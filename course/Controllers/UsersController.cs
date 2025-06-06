using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList

namespace course.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        // Отображает список всех пользователей.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        // Отображает детали конкретного пользователя по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound(); // Если пользователь не найден, возвращаем 404
            }

            return View(user);
        }

        // GET: Users/Create
        // Отображает форму для создания нового пользователя.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // Обрабатывает отправку формы для создания нового пользователя.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        public async Task<IActionResult> Create([Bind("Login,Email,PasswordHash,BlockStatus,Rating")] User user)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                // В реальном приложении:
                // 1. Пароль должен быть хеширован (например, с помощью BCrypt.Net-Core или AspNetCore.Cryptography.KeyDerivation)
                // user.PasswordHash = YourPasswordHasher.HashPassword(user.PasswordHash);
                
                // Устанавливаем дату регистрации
                user.RegistrationDate = DateTime.UtcNow;
                
                _context.Add(user); // Добавляем пользователя в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно создан.";
                return RedirectToAction(nameof(Index)); // Перенаправляем на список пользователей
            }
            TempData["ErrorMessage"] = "Ошибка при создании пользователя.";
            return View(user); // Если модель невалидна, возвращаем форму с ошибками
        }

        // GET: Users/Edit/5
        // Отображает форму для редактирования существующего пользователя.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id); // Находим пользователя по Id
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // Обрабатывает отправку формы для редактирования пользователя.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Login,Email,PasswordHash,RegistrationDate,BlockStatus,Rating")] User user)
        {
            if (id != user.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Внимание: если вы позволяете редактировать PasswordHash через эту форму,
                    // его нужно будет снова хешировать, если он изменился.
                    // Либо создайте отдельное действие для изменения пароля.
                    _context.Update(user); // Обновляем пользователя в контексте
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Пробрасываем ошибку, если это не конфликт параллельного доступа
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Ошибка при обновлении пользователя.";
            return View(user);
        }

        // GET: Users/Delete/5
        // Отображает страницу подтверждения удаления пользователя.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        // Обрабатывает подтверждение удаления пользователя.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user); // Удаляем пользователя из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            TempData["SuccessMessage"] = "Пользователь успешно удален.";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования пользователя
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // =====================================================================
        // НОВЫЕ ДЕЙСТВИЯ ДЛЯ УПРАВЛЕНИЯ СТАТУСОМ АДМИНИСТРАТОРА
        // =====================================================================

        // GET: Users/ManageAdministratorStatus/5
        // Отображает форму для управления статусом администратора конкретного пользователя.
        public async Task<IActionResult> ManageAdministratorStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administrators.FirstOrDefaultAsync(a => a.UserId == id);

            var viewModel = new UserAdminStatusViewModel
            {
                UserId = user.Id,
                UserName = user.Login,
                Email = user.Email,
                IsAdministrator = (administrator != null),
                AccessLevel = administrator?.AccessLevel,
                AssignmentDate = administrator?.AssignmentDate
            };

            return View(viewModel);
        }

        // POST: Users/ManageAdministratorStatus/5
        // Обрабатывает отправку формы для управления статусом администратора.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageAdministratorStatus(int id, [Bind("UserId,IsAdministrator,AccessLevel")] UserAdminStatusViewModel viewModel)
        {
            if (id != viewModel.UserId)
            {
                return NotFound();
            }

            // Получаем оригинальные данные пользователя, чтобы убедиться, что Login и Email не были подделаны
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Добавляем эти значения обратно в viewModel для корректного отображения в случае ошибки
            viewModel.UserName = user.Login;
            viewModel.Email = user.Email;

            // Если пользователь пытается сделать себя администратором, но не указал AccessLevel, или указал 0
            if (viewModel.IsAdministrator && (viewModel.AccessLevel == null || viewModel.AccessLevel <= 0))
            {
                ModelState.AddModelError("AccessLevel", "Для администратора необходимо указать уровень доступа больше 0.");
            }
            // Если пользователь не является администратором, AccessLevel должен быть null
            else if (!viewModel.IsAdministrator)
            {
                viewModel.AccessLevel = null;
            }


            if (ModelState.IsValid)
            {
                var existingAdmin = await _context.Administrators.FirstOrDefaultAsync(a => a.UserId == id);

                if (viewModel.IsAdministrator)
                {
                    // Если пользователь должен быть администратором
                    if (existingAdmin == null)
                    {
                        // Создаем новую запись Administrator
                        var newAdmin = new Administrator
                        {
                            UserId = viewModel.UserId,
                            AccessLevel = viewModel.AccessLevel ?? 1, // Используем предоставленный AccessLevel или 1 по умолчанию
                            AssignmentDate = DateTime.UtcNow
                        };
                        _context.Administrators.Add(newAdmin);
                        TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно назначен администратором.";
                    }
                    else
                    {
                        // Обновляем существующую запись Administrator
                        existingAdmin.AccessLevel = viewModel.AccessLevel ?? existingAdmin.AccessLevel; // Обновляем уровень доступа
                        _context.Administrators.Update(existingAdmin);
                        TempData["SuccessMessage"] = $"Уровень доступа администратора '{user.Login}' успешно обновлен.";
                    }
                }
                else
                {
                    // Если пользователь больше НЕ должен быть администратором
                    if (existingAdmin != null)
                    {
                        // Удаляем запись Administrator
                        _context.Administrators.Remove(existingAdmin);
                        TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно лишен статуса администратора.";
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "Users"); // Перенаправляем на список пользователей
            }

            // Если ModelState не валиден, повторно отображаем представление с ошибками
            // Передаем текущую дату назначения, если она была (для повторного отображения формы)
            var currentAdmin = await _context.Administrators.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == id);
            viewModel.AssignmentDate = currentAdmin?.AssignmentDate;
            return View(viewModel);
        }
    }
}
