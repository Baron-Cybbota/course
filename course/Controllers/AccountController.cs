using Microsoft.AspNetCore.Mvc;
using course.Data;
using course.Models;
using course.Models.ViewModels; // Подключаем наши ViewModels
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net; // Для хеширования паролей
using Microsoft.EntityFrameworkCore; // ДОБАВЛЕНО: Для методов AnyAsync, FirstOrDefaultAsync и т.д.

namespace course.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли пользователь с таким логином или email
                if (await _context.Users.AnyAsync(u => u.Login == model.Login || u.Email == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с таким логином или Email уже существует.");
                    return View(model);
                }

                // Хешируем пароль
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var user = new User
                {
                    Login = model.Login,
                    Email = model.Email,
                    PasswordHash = hashedPassword,
                    RegistrationDate = DateTime.UtcNow,
                    BlockStatus = false, // По умолчанию пользователь не заблокирован
                    Rating = 0 // Начальный рейтинг
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Автоматический вход после регистрации
                await Authenticate(user);

                TempData["SuccessMessage"] = $"Добро пожаловать, {user.Login}! Вы успешно зарегистрированы.";
                return RedirectToAction("Index", "Home"); // Перенаправляем на главную страницу
            }
            TempData["ErrorMessage"] = "Ошибка при регистрации. Проверьте введенные данные.";
            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Поиск пользователя по логину или email
                var user = await _context.Users
                                        .FirstOrDefaultAsync(u => u.Login == model.LoginOrEmail || u.Email == model.LoginOrEmail);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    if (user.BlockStatus)
                    {
                        ModelState.AddModelError(string.Empty, "Ваша учетная запись заблокирована.");
                        return View(model);
                    }

                    await Authenticate(user, model.RememberMe); // Выполняем вход
                    TempData["SuccessMessage"] = $"С возвращением, {user.Login}!";
                    return RedirectToLocal(returnUrl); // Перенаправляем на URL, с которого пришли, или на главную
                }

                ModelState.AddModelError(string.Empty, "Неверный логин/Email или пароль.");
            }
            TempData["ErrorMessage"] = "Ошибка входа. Проверьте введенные данные.";
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Вы успешно вышли из системы.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Вспомогательный метод для выполнения входа пользователя
        private async Task Authenticate(User user, bool rememberMe = false)
        {
            // Создаем Claims для пользователя
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID пользователя
                new Claim(ClaimTypes.Name, user.Login), // Логин пользователя
                new Claim(ClaimTypes.Email, user.Email) // Email пользователя
            };

            // Добавляем Claim роли, если пользователь является администратором
            var administrator = await _context.Administrators.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (administrator != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrator")); // Роль "Administrator"
                // Можно добавить AccessLevel как отдельный Claim
                claims.Add(new Claim("AccessLevel", administrator.AccessLevel.ToString()));
            }
            // Здесь вы можете добавить другие роли (например, "Модератор"), если они у вас есть
            // посредством проверки других таблиц или полей в таблице Users.

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe, // Сохранять ли куки после закрытия браузера
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(30) // Время жизни куки
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // Вспомогательный метод для безопасного перенаправления после входа
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
