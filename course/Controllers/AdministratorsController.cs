using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()

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
            // В реальном приложении, возможно, вы захотите подгружать данные пользователя:
            // return View(await _context.Administrators.Include(a => a.User).ToListAsync());
            return View(await _context.Administrators.ToListAsync());
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
                // В реальном приложении, возможно, вы захотите подгружать данные пользователя:
                // .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null)
            {
                return NotFound(); // Если администратор не найден, возвращаем 404
            }

            return View(administrator);
        }

        // GET: Administrators/Create
        // Отображает форму для создания нового администратора.
        public async Task<IActionResult> Create()
        {
            // Для выбора UserId, передаем список пользователей, которые еще не являются администраторами.
            // В реальном UI это был бы выпадающий список.
            ViewBag.AvailableUsers = await _context.Users
                                                    .Where(u => !_context.Administrators.Any(a => a.UserId == u.Id))
                                                    .ToListAsync();
            return View();
        }

        // POST: Administrators/Create
        // Обрабатывает отправку формы для создания нового администратора.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        public async Task<IActionResult> Create([Bind("UserId,AccessLevel")] Administrator administrator)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                // Проверяем, не является ли пользователь уже администратором.
                // Это предотвратит ошибку уникального индекса.
                if (await _context.Administrators.AnyAsync(a => a.UserId == administrator.UserId))
                {
                    ModelState.AddModelError("UserId", "Этот пользователь уже является администратором.");
                    ViewBag.AvailableUsers = await _context.Users
                                                            .Where(u => !_context.Administrators.Any(a => a.UserId == u.Id))
                                                            .ToListAsync();
                    return View(administrator);
                }

                administrator.AssignmentDate = DateTime.UtcNow; // Устанавливаем дату назначения
                
                _context.Add(administrator); // Добавляем администратора в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список администраторов
            }

            // Если модель невалидна, возвращаем форму с ошибками
            ViewBag.AvailableUsers = await _context.Users
                                                    .Where(u => !_context.Administrators.Any(a => a.UserId == u.Id))
                                                    .ToListAsync();
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

            var administrator = await _context.Administrators.FindAsync(id); // Находим администратора по Id
            if (administrator == null)
            {
                return NotFound();
            }

            // Для редактирования, возможно, вы захотите дать выбрать другого пользователя,
            // но обычно UserId для существующего администратора не меняется.
            // Если UserId не подлежит изменению, то его не нужно включать в форму редактирования.
            // Если вы хотите, чтобы UserId был редактируемым, вам нужно будет обрабатывать уникальность.
            ViewBag.AvailableUsers = await _context.Users.ToListAsync(); // Или только текущий пользователь
            return View(administrator);
        }

        // POST: Administrators/Edit/5
        // Обрабатывает отправку формы для редактирования администратора.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind("Id,UserId,AssignmentDate,AccessLevel") - убедитесь, что все поля, которые могут быть изменены, включены.
        // UserId и AssignmentDate обычно не редактируются после создания.
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,AssignmentDate,AccessLevel")] Administrator administrator)
        {
            if (id != administrator.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            // Важно: если UserId редактируется, нужно повторно проверять уникальность,
            // а также удостовериться, что старый UserId не остается в базе данных, если он больше не админ.
            // Для простоты, предполагается, что UserId не меняется в форме редактирования.

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administrator); // Обновляем администратора в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!AdministratorExists(administrator.Id))
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
            ViewBag.AvailableUsers = await _context.Users.ToListAsync();
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
                // В реальном приложении, возможно, вы захотите подгружать данные пользователя:
                // .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null)
            {
                return NotFound();
            }

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
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования администратора
        private bool AdministratorExists(int id)
        {
            return _context.Administrators.Any(e => e.Id == id);
        }
    }
}
