using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList
using System.Collections.Generic; // For Dictionary

namespace course.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Messages
        // Отображает список всех сообщений чата.
        public async Task<IActionResult> Index()
        {
            // Fetch messages along with related Event Name and User Login
            var messagesWithDetails = await _context.Messages
                .Select(m => new
                {
                    Message = m,
                    EventName = _context.Events.Where(e => e.IdEvent == m.IdEvent).Select(e => e.Name).FirstOrDefault(), // Assuming Event model has IdEvent and Name
                    UserLogin = _context.Users.Where(u => u.IdUser == m.IdUser).Select(u => u.Login).FirstOrDefault() // Assuming User model has IdUser and Login
                })
                .ToListAsync();

            var eventNames = new Dictionary<int, string>();
            var userLogins = new Dictionary<int, string>();

            foreach (var item in messagesWithDetails)
            {
                // Note: IdEvent is nullable as per your DbContext config. Handle nulls.
                if (item.Message.IdEvent.HasValue && item.Message.IdEvent.Value != 0 && item.EventName != null)
                {
                    eventNames[item.Message.IdEvent.Value] = item.EventName;
                }
                if (item.Message.IdUser != 0 && item.UserLogin != null)
                {
                    userLogins[item.Message.IdUser] = item.UserLogin;
                }
            }

            ViewData["EventNames"] = eventNames;
            ViewData["UserLogins"] = userLogins;

            return View(messagesWithDetails.Select(x => x.Message).ToList());
        }

        // GET: Messages/Details/5
        // Отображает детали конкретного сообщения чата по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.IdMessage == id); // CORRECTED: Use IdMessage
            if (message == null)
            {
                return NotFound(); // Если сообщение не найдено, возвращаем 404
            }

            // Fetch related Event Name and User Login
            var eventName = await _context.Events
                                            .Where(e => e.IdEvent == message.IdEvent)
                                            .Select(e => e.Name)
                                            .FirstOrDefaultAsync();
            var userLogin = await _context.Users
                                          .Where(u => u.IdUser == message.IdUser)
                                          .Select(u => u.Login)
                                          .FirstOrDefaultAsync();

            ViewData["EventName"] = eventName ?? "Неизвестное событие/чат";
            ViewData["UserLogin"] = userLogin ?? "Неизвестный отправитель";

            return View(message);
        }

        // GET: Messages/Create
        // Отображает форму для создания нового сообщения чата.
        public async Task<IActionResult> Create()
        {
            // Populate SelectLists for IdEvent (as chat) and IdUser (sender)
            ViewBag.IdEvent = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name"); // CORRECTED: Use IdEvent and Name
            ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login"); // CORRECTED: Use IdUser and Login
            return View();
        }

        // POST: Messages/Create
        // Обрабатывает отправку формы для создания нового сообщения чата.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // CORRECTED: Bind IdEvent and IdUser. SendDate and IdMessage are server-set.
        public async Task<IActionResult> Create([Bind("IdEvent,IdUser,Content,IsRead")] Message message) // CORRECTED: Parameter name to 'message'
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                message.SendDate = DateTime.UtcNow; // Устанавливаем дату отправки
                
                _context.Add(message); // Добавляем сообщение в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                TempData["SuccessMessage"] = "Сообщение успешно добавлено.";
                return RedirectToAction(nameof(Index)); // Перенаправляем на список сообщений
            }
            // Если модель невалидна, возвращаем форму с ошибками, повторно заполняя ViewBag
            ViewBag.IdEvent = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name", message.IdEvent); // CORRECTED: Use IdEvent
            ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", message.IdUser); // CORRECTED: Use IdUser
            TempData["ErrorMessage"] = "Ошибка при создании сообщения. Проверьте введенные данные.";
            return View(message);
        }

        // GET: Messages/Edit/5
        // Отображает форму для редактирования существующего сообщения чата.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id); // Will implicitly use IdMessage
            if (message == null)
            {
                return NotFound();
            }
            // Populate SelectLists for IdEvent and IdUser (even if not directly editable by user)
            ViewBag.IdEvent = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name", message.IdEvent); // CORRECTED: Use IdEvent
            ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", message.IdUser); // CORRECTED: Use IdUser
            return View(message);
        }

        // POST: Messages/Edit/5
        // Обрабатывает отправку формы для редактирования сообщения чата.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CORRECTED: Bind IdMessage, Content, and IsRead. SendDate, IdEvent, IdUser should be preserved.
        public async Task<IActionResult> Edit(int id, [Bind("IdMessage,Content,IsRead")] Message message) // CORRECTED: IdMessage in Bind, parameter name
        {
            if (id != message.IdMessage) // CORRECTED: Check against IdMessage
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Load the existing entry to preserve immutable fields
                    var existingMessage = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.IdMessage == id); // CORRECTED: Use IdMessage
                    if (existingMessage == null)
                    {
                        return NotFound();
                    }

                    // Transfer immutable fields from the existing entity
                    message.SendDate = existingMessage.SendDate;
                    message.IdEvent = existingMessage.IdEvent; // Preserve original Event/Chat
                    message.IdUser = existingMessage.IdUser; // Preserve original Sender

                    _context.Update(message); // Обновляем сообщение в контексте
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Сообщение успешно обновлено.";
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!MessageExists(message.IdMessage)) // CORRECTED: Check against IdMessage
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
            ViewBag.IdEvent = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name", message.IdEvent); // CORRECTED: Use IdEvent
            ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", message.IdUser); // CORRECTED: Use IdUser
            TempData["ErrorMessage"] = "Ошибка при обновлении сообщения. Проверьте введенные данные.";
            return View(message);
        }

        // GET: Messages/Delete/5
        // Отображает страницу подтверждения удаления сообщения чата.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.IdMessage == id); // CORRECTED: Use IdMessage
            if (message == null)
            {
                return NotFound();
            }

            // Fetch related Event Name and User Login explicitly
            var eventName = await _context.Events
                                            .Where(e => e.IdEvent == message.IdEvent)
                                            .Select(e => e.Name)
                                            .FirstOrDefaultAsync();
            var userLogin = await _context.Users
                                          .Where(u => u.IdUser == message.IdUser)
                                          .Select(u => u.Login)
                                          .FirstOrDefaultAsync();

            ViewData["EventName"] = eventName ?? "Неизвестное событие/чат";
            ViewData["UserLogin"] = userLogin ?? "Неизвестный отправитель";

            return View(message);
        }

        // POST: Messages/Delete/5
        // Обрабатывает подтверждение удаления сообщения чата.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id); // Will implicitly use IdMessage
            if (message != null)
            {
                _context.Messages.Remove(message); // Удаляем сообщение из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            TempData["SuccessMessage"] = "Сообщение успешно удалено.";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования сообщения чата
        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.IdMessage == id); // CORRECTED: Use IdMessage
        }
    }
}