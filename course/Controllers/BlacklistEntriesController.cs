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
    public class BlacklistEntriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlacklistEntriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BlacklistEntries
        // Отображает список всех записей в черном списке.
        public async Task<IActionResult> Index()
        {
            return View(await _context.BlacklistEntries.ToListAsync());
        }

        // GET: BlacklistEntries/Details/5
        // Отображает детали конкретной записи черного списка по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var blacklistEntry = await _context.BlacklistEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blacklistEntry == null)
            {
                return NotFound(); // Если запись не найдена, возвращаем 404
            }

            return View(blacklistEntry);
        }

        // GET: BlacklistEntries/Create
        // Отображает форму для создания новой записи в черном списке.
        public async Task<IActionResult> Create()
        {
            // Для выбора AdministratorId: список администраторов (их UserIds и логины)
            var administrators = await _context.Administrators.ToListAsync();
            var adminUserIds = administrators.Select(a => a.UserId).ToList();
            var adminUsers = await _context.Users
                                            .Where(u => adminUserIds.Contains(u.Id))
                                            .Select(u => new { u.Id, Text = $"{u.Login} (ID: {u.Id})" })
                                            .ToListAsync();
            ViewBag.AdministratorId = new SelectList(adminUsers, "Id", "Text");

            // Для выбора UserId для блокировки: список всех пользователей, которые еще не заблокированы
            var blockedUserIds = await _context.BlacklistEntries.Select(b => b.UserId).ToListAsync();
            var availableUsers = await _context.Users
                                            .Where(u => !blockedUserIds.Contains(u.Id))
                                            .Select(u => new { u.Id, Text = $"{u.Login} (ID: {u.Id})" })
                                            .ToListAsync();
            ViewBag.UserId = new SelectList(availableUsers, "Id", "Text");

            return View();
        }

        // POST: BlacklistEntries/Create
        // Обрабатывает отправку формы для создания новой записи в черном списке.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // Поля, которые пользователь может ввести. BlockDate устанавливается сервером.
        public async Task<IActionResult> Create([Bind("AdministratorId,UserId,Reason,BlockDuration")] BlacklistEntry blacklistEntry)
        {
            // Проверяем, не заблокирован ли пользователь уже.
            // Это предотвратит ошибку уникального индекса.
            if (await _context.BlacklistEntries.AnyAsync(b => b.UserId == blacklistEntry.UserId))
            {
                ModelState.AddModelError("UserId", "Этот пользователь уже находится в черном списке.");
            }

            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                blacklistEntry.BlockDate = DateTime.UtcNow; // Устанавливаем дату блокировки
                
                _context.Add(blacklistEntry); // Добавляем запись в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список записей
            }
            // Если модель невалидна, возвращаем форму с ошибками, повторно заполняя ViewBag
            var administrators = await _context.Administrators.ToListAsync();
            var adminUserIds = administrators.Select(a => a.UserId).ToList();
            var adminUsers = await _context.Users
                                            .Where(u => adminUserIds.Contains(u.Id))
                                            .Select(u => new { u.Id, Text = $"{u.Login} (ID: {u.Id})" })
                                            .ToListAsync();
            ViewBag.AdministratorId = new SelectList(adminUsers, "Id", "Text", blacklistEntry.AdministratorId);

            var blockedUserIds = await _context.BlacklistEntries.Select(b => b.UserId).ToListAsync();
            var availableUsers = await _context.Users
                                            .Where(u => !blockedUserIds.Contains(u.Id) || u.Id == blacklistEntry.UserId) // Включить текущего пользователя, если он уже выбран
                                            .Select(u => new { u.Id, Text = $"{u.Login} (ID: {u.Id})" })
                                            .ToListAsync();
            ViewBag.UserId = new SelectList(availableUsers, "Id", "Text", blacklistEntry.UserId);

            return View(blacklistEntry);
        }

        // GET: BlacklistEntries/Edit/5
        // Отображает форму для редактирования существующей записи в черном списке.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blacklistEntry = await _context.BlacklistEntries.FindAsync(id); // Находим запись по Id
            if (blacklistEntry == null)
            {
                return NotFound();
            }
            // Для редактирования AdministratorId
            var administrators = await _context.Administrators.ToListAsync();
            var adminUserIds = administrators.Select(a => a.UserId).ToList();
            var adminUsers = await _context.Users
                                            .Where(u => adminUserIds.Contains(u.Id))
                                            .Select(u => new { u.Id, Text = $"{u.Login} (ID: {u.Id})" })
                                            .ToListAsync();
            ViewBag.AdministratorId = new SelectList(adminUsers, "Id", "Text", blacklistEntry.AdministratorId);

            // UserId обычно не меняется при редактировании записи черного списка,
            // но если вам нужно изменить AdministratorId или BlockDuration, они будут доступны.
            // ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", blacklistEntry.UserId); // Может быть только для отображения
            return View(blacklistEntry);
        }

        // POST: BlacklistEntries/Edit/5
        // Обрабатывает отправку формы для редактирования записи черного списка.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind("Id,AdministratorId,UserId,Reason,BlockDate,BlockDuration") - убедитесь, что все поля, которые могут быть изменены, включены.
        // BlockDate и UserId обычно не редактируются после создания.
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdministratorId,UserId,Reason,BlockDate,BlockDuration")] BlacklistEntry blacklistEntry)
        {
            if (id != blacklistEntry.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blacklistEntry); // Обновляем запись в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!BlacklistEntryExists(blacklistEntry.Id))
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
            // Если модель невалидна, возвращаем форму с ошибками
            var administrators = await _context.Administrators.ToListAsync();
            var adminUserIds = administrators.Select(a => a.UserId).ToList();
            var adminUsers = await _context.Users
                                            .Where(u => adminUserIds.Contains(u.Id))
                                            .Select(u => new { u.Id, Text = $"{u.Login} (ID: {u.Id})" })
                                            .ToListAsync();
            ViewBag.AdministratorId = new SelectList(adminUsers, "Id", "Text", blacklistEntry.AdministratorId);
            // ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", blacklistEntry.UserId);
            return View(blacklistEntry);
        }

        // GET: BlacklistEntries/Delete/5
        // Отображает страницу подтверждения удаления записи черного списка.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blacklistEntry = await _context.BlacklistEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blacklistEntry == null)
            {
                return NotFound();
            }

            // Добавляем для отображения: AdministratorLogin и UserLogin
            var adminUser = await _context.Users.FindAsync(blacklistEntry.AdministratorId);
            var blockedUser = await _context.Users.FindAsync(blacklistEntry.UserId);

            ViewData["AdministratorLogin"] = adminUser?.Login ?? "Неизвестный администратор";
            ViewData["UserLogin"] = blockedUser?.Login ?? "Неизвестный пользователь";

            return View(blacklistEntry);
        }

        // POST: BlacklistEntries/Delete/5
        // Обрабатывает подтверждение удаления записи черного списка.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blacklistEntry = await _context.BlacklistEntries.FindAsync(id);
            if (blacklistEntry != null)
            {
                _context.BlacklistEntries.Remove(blacklistEntry); // Удаляем запись из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования записи черного списка
        private bool BlacklistEntryExists(int id)
        {
            return _context.BlacklistEntries.Any(e => e.Id == id);
        }
    }
}
