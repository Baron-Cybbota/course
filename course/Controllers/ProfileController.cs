using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using course.Models.ViewModels;
using System.Security.Claims; // Для работы с Claims
using System.Threading.Tasks;
using BCrypt.Net; // Для хеширования и проверки паролей
using System.Linq; // Для OrderByDescending и ToListAsync
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication; // ДОБАВЛЕНО: Для HttpContext extension methods

namespace course.Controllers
{
    // Атрибут [Authorize] гарантирует, что только аутентифицированные пользователи могут получить доступ к этому контроллеру
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Profile/Index
        // Отображает профиль текущего залогиненного пользователя
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                // Если ID пользователя не найден или некорректен (что не должно произойти для аутентифицированного пользователя)
                return RedirectToAction("Login", "Account"); // Перенаправляем на страницу входа
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                // Пользователь не найден в базе данных, хотя Claim есть (очень редкий случай)
                await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ErrorMessage"] = "Ваша учетная запись не найдена.";
                return RedirectToAction("Login", "Account");
            }

            // Загружаем последние посты этого пользователя
            var userPosts = await _context.Posts
                                          .Where(p => p.AuthorId == userId)
                                          .OrderByDescending(p => p.CreationDate)
                                          .Take(5) // Ограничиваем количество постов для отображения
                                          .ToListAsync();

            ViewBag.UserPosts = userPosts; // Передаем посты в ViewBag

            // Передаем модель пользователя в представление
            return View("Profile", user); // Используем представление Profile.cshtml
        }

        // GET: Profile/Details/{id}
        // Отображает профиль пользователя по заданному ID (для просмотра чужих профилей)
        // Если ID не указан, перенаправляет на свой собственный профиль
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                // Если ID не предоставлен, отображаем профиль текущего пользователя
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(); // Пользователь не найден
            }

            // Загружаем последние посты этого пользователя
            var userPosts = await _context.Posts
                                          .Where(p => p.AuthorId == id.Value)
                                          .OrderByDescending(p => p.CreationDate)
                                          .Take(5) // Ограничиваем количество постов для отображения
                                          .ToListAsync();

            ViewBag.UserPosts = userPosts; // Передаем посты в ViewBag

            return View("Profile", user); // Используем представление Profile.cshtml
        }


        // GET: Profile/Edit
        // Отображает форму для редактирования данных профиля текущего залогиненного пользователя
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileEditViewModel
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                BlockStatus = user.BlockStatus,
                Rating = user.Rating,
                RegistrationDate = user.RegistrationDate
            };

            return View(viewModel);
        }

        // POST: Profile/Edit
        // Обрабатывает отправку формы для редактирования данных профиля
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId) || userId != model.Id)
            {
                // Попытка редактирования чужого профиля или неверный ID
                return Forbid(); // Запрещаем доступ
            }

            if (ModelState.IsValid)
            {
                var userToUpdate = await _context.Users.FindAsync(model.Id);
                if (userToUpdate == null)
                {
                    return NotFound();
                }

                // Проверяем уникальность логина и email, если они изменились
                if (await _context.Users.AnyAsync(u => u.Id != model.Id && (u.Login == model.Login || u.Email == model.Email)))
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с таким логином или Email уже существует.");
                    return View(model);
                }

                userToUpdate.Login = model.Login;
                userToUpdate.Email = model.Email;
                // BlockStatus и Rating не изменяются через эту форму, они для админов
                // userToUpdate.BlockStatus = model.BlockStatus;
                // userToUpdate.Rating = model.Rating;

                try
                {
                    _context.Update(userToUpdate);
                    await _context.SaveChangesAsync();

                    // Обновляем Claims, если изменились Login или Email
                    await Authenticate(userToUpdate, rememberMe: User.Identity?.IsAuthenticated == true); // Проверяем, был ли пользователь "запомнен"

                    TempData["SuccessMessage"] = "Профиль успешно обновлен!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            TempData["ErrorMessage"] = "Ошибка при обновлении профиля. Проверьте введенные данные.";
            return View(model);
        }

        // GET: Profile/ChangePassword
        // Отображает форму для изменения пароля текущего залогиненного пользователя
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        // Обрабатывает отправку формы для изменения пароля
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                // Проверяем текущий пароль
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "Неверный текущий пароль.");
                    return View(model);
                }

                // Хешируем новый пароль
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Пароль успешно изменен!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            TempData["ErrorMessage"] = "Ошибка при изменении пароля. Проверьте введенные данные.";
            return View(model);
        }

        // Вспомогательный метод для выполнения входа пользователя (дублируется из AccountController для удобства)
        private async Task Authenticate(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var administrator = await _context.Administrators.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (administrator != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                claims.Add(new Claim("AccessLevel", administrator.AccessLevel.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
