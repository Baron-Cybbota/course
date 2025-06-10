using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList, although not strictly needed for AvailableUsers as it's just a List.

namespace course.Controllers
{
    public class AdministratorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdministratorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Administrators
        // Отображает список всех администраторов.
        public async Task<IActionResult> Index()
        {
            var administrators = await _context.Administrators.ToListAsync();

            // Collect all unique IdUser from administrators
            var userIds = administrators.Select(a => a.IdUser).Distinct().ToList();

            // Fetch User logins using the collected Ids
            var userLogins = await _context.Users
                                            .Where(u => userIds.Contains(u.IdUser))
                                            .ToDictionaryAsync(u => u.IdUser, u => u.Login);

            ViewData["UserLogins"] = userLogins; // Pass dictionary to ViewData

            return View(administrators);
        }

        // GET: Administrators/Details/5
        // Отображает детали конкретного администратора по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var administrator = await _context.Administrators
                                            .FirstOrDefaultAsync(m => m.IdAdministrator == id); // Using IdAdministrator as primary key
            if (administrator == null)
            {
                return NotFound(); // Если администратор не найден, возвращаем 404
            }

            // Fetch the associated user's login
            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == administrator.IdUser);
            ViewData["UserLogin"] = user?.Login ?? "Неизвестный пользователь"; // Pass user login to ViewData

            return View(administrator);
        }

        // GET: Administrators/Create
        // Отображает форму для создания нового администратора.
        public async Task<IActionResult> Create()
        {
            // Для выбора IdUser, передаем список пользователей, которые еще не являются администраторами.
            // In a real UI this would be a dropdown.
            // Using SelectList to prepare data for a dropdown in the view
            ViewBag.AvailableUsers = new SelectList(
                await _context.Users
                              .Where(u => !_context.Administrators.Any(a => a.IdUser == u.IdUser))
                              .ToListAsync(),
                "IdUser", // Value property for SelectListItem
                "Login"   // Text property for SelectListItem
            );
            return View();
        }

        // POST: Administrators/Create
        // Обрабатывает отправку формы для создания нового администратора.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        public async Task<IActionResult> Create([Bind("IdUser")] Administrator administrator)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                // Проверяем, не является ли пользователь уже администратором.
                // Это предотвратит ошибку уникального индекса.
                if (await _context.Administrators.AnyAsync(a => a.IdUser == administrator.IdUser))
                {
                    ModelState.AddModelError("IdUser", "Этот пользователь уже является администратором.");
                    // Re-populate ViewBag if model state is invalid
                    ViewBag.AvailableUsers = new SelectList(
                        await _context.Users
                                      .Where(u => !_context.Administrators.Any(a => a.IdUser == u.IdUser))
                                      .ToListAsync(),
                        "IdUser",
                        "Login",
                        administrator.IdUser // Select the invalid user if it was chosen
                    );
                    return View(administrator);
                }

                administrator.AssignmentDate = DateTime.UtcNow; // Устанавливаем дату назначения

                _context.Add(administrator); // Добавляем администратора в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                TempData["SuccessMessage"] = "Администратор успешно добавлен.";
                return RedirectToAction(nameof(Index)); // Перенаправляем на список администраторов
            }

            // Если модель невалидна, возвращаем форму с ошибками
            ViewBag.AvailableUsers = new SelectList(
                await _context.Users
                              .Where(u => !_context.Administrators.Any(a => a.IdUser == u.IdUser))
                              .ToListAsync(),
                "IdUser",
                "Login",
                administrator.IdUser
            );
            TempData["ErrorMessage"] = "Ошибка при создании администратора. Проверьте введенные данные.";
            return View(administrator);
        }

        // GET: Administrators/Edit/5
        // Отображает форму для редактирования существующего администратора.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administrators.FindAsync(id);
            if (administrator == null)
            {
                return NotFound();
            }

            // For editing, you would typically not change the UserId of an existing administrator.
            // If you want to allow changing the associated user, you need to handle uniqueness checks.
            // Also, consider if only specific users should be available for selection.
            // We pass all users for simplicity, assuming the IdUser is displayed but not typically changed via dropdown.
            // If it should be a dropdown, use SelectList as in Create().
            ViewBag.AllUsers = await _context.Users.ToListAsync(); // This is just a list of User objects
                                                                   // If you want a dropdown, it should be:
                                                                   // ViewBag.AllUsers = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", administrator.IdUser);
            return View(administrator);
        }

        // POST: Administrators/Edit/5
        // Обрабатывает отправку формы для редактирования администратора.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAdministrator,IdUser,AssignmentDate")] Administrator administrator)
        {
            if (id != administrator.IdAdministrator)
            {
                return NotFound();
            }

            // IMPORTANT: If IdUser is allowed to be edited here, you MUST re-validate its uniqueness
            // against other administrators, excluding the current one.
            // For simplicity, I'm assuming IdUser is NOT intended to be changed via the edit form.
            // If it can be changed, uncomment and adapt the logic below:
            /*
            var existingAdministrator = await _context.Administrators.AsNoTracking().FirstOrDefaultAsync(a => a.IdAdministrator == id);
            if (existingAdministrator != null && existingAdministrator.IdUser != administrator.IdUser)
            {
                if (await _context.Administrators.AnyAsync(a => a.IdUser == administrator.IdUser && a.IdAdministrator != administrator.IdAdministrator))
                {
                    ModelState.AddModelError("IdUser", "Этот пользователь уже назначен администратором.");
                    ViewBag.AllUsers = await _context.Users.ToListAsync(); // Or a SelectList
                    return View(administrator);
                }
            }
            */

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administrator); // Обновляем администратора в контексте
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Администратор успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!AdministratorExists(administrator.IdAdministrator))
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
            // If model is invalid, return the form with errors
            ViewBag.AllUsers = await _context.Users.ToListAsync(); // Or a SelectList
            TempData["ErrorMessage"] = "Ошибка при обновлении администратора. Проверьте введенные данные.";
            return View(administrator);
        }

        // GET: Administrators/Delete/5
        // Отображает страницу подтверждения удаления администратора.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administrators
                                            .FirstOrDefaultAsync(m => m.IdAdministrator == id);
            if (administrator == null)
            {
                return NotFound();
            }

            // Fetch the associated user's login for display on the delete confirmation page
            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == administrator.IdUser);
            ViewData["UserLogin"] = user?.Login ?? "Неизвестный пользователь";

            return View(administrator);
        }

        // POST: Administrators/Delete/5
        // Обрабатывает подтверждение удаления администратора.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var administrator = await _context.Administrators.FindAsync(id);
            if (administrator != null)
            {
                _context.Administrators.Remove(administrator); // Удаляем администратора из контекста
            }

            await _context.SaveChangesAsync(); // Сохраняем изменения
            TempData["SuccessMessage"] = "Администратор успешно удален.";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования администратора
        private bool AdministratorExists(int id)
        {
            return _context.Administrators.Any(e => e.IdAdministrator == id);
        }
    }
}