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
    public class ChatMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ChatMessages
        // Отображает список всех сообщений чата.
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChatMessages.ToListAsync());
        }

        // GET: ChatMessages/Details/5
        // Отображает детали конкретного сообщения чата по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var chatMessage = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chatMessage == null)
            {
                return NotFound(); // Если сообщение не найдено, возвращаем 404
            }

            return View(chatMessage);
        }

        // GET: ChatMessages/Create
        // Отображает форму для создания нового сообщения чата.
        public async Task<IActionResult> Create()
        {
            // Для выбора ChatId и SenderId, если это требуется на форме.
            // ViewBag.ChatId = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name");
            // ViewBag.SenderId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login");
            return View();
        }

        // POST: ChatMessages/Create
        // Обрабатывает отправку формы для создания нового сообщения чата.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // Только эти поля, SentDate будет установлена сервером
        public async Task<IActionResult> Create([Bind("ChatId,SenderId,Content,IsRead")]  ChatMessage chatMessage)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                chatMessage.SentDate = DateTime.UtcNow; // Устанавливаем дату отправки
                
                _context.Add(chatMessage); // Добавляем сообщение в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список сообщений
            }
            // Если модель невалидна, возвращаем форму с ошибками
            // ViewBag.ChatId = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name", chatMessage.ChatId);
            // ViewBag.SenderId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chatMessage.SenderId);
            return View(chatMessage);
        }

        // GET: ChatMessages/Edit/5
        // Отображает форму для редактирования существующего сообщения чата.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatMessage = await _context.ChatMessages.FindAsync(id); // Находим сообщение по Id
            if (chatMessage == null)
            {
                return NotFound();
            }
            // Для редактирования ChatId или SenderId, если они должны быть редактируемыми.
            // ViewBag.ChatId = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name", chatMessage.ChatId);
            // ViewBag.SenderId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chatMessage.SenderId);
            return View(chatMessage);
        }

        // POST: ChatMessages/Edit/5
        // Обрабатывает отправку формы для редактирования сообщения чата.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind("Id,ChatId,SenderId,Content,SentDate,IsRead") - убедитесь, что все поля, которые могут быть изменены, включены.
        // SentDate обычно не редактируется.
        public async Task<IActionResult> Edit(int id, [Bind("Id,ChatId,SenderId,Content,SentDate,IsRead")] ChatMessage chatMessage)
        {
            if (id != chatMessage.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chatMessage); // Обновляем сообщение в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!ChatMessageExists(chatMessage.Id))
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
            // ViewBag.ChatId = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name", chatMessage.ChatId);
            // ViewBag.SenderId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chatMessage.SenderId);
            return View(chatMessage);
        }

        // GET: ChatMessages/Delete/5
        // Отображает страницу подтверждения удаления сообщения чата.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatMessage = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chatMessage == null)
            {
                return NotFound();
            }

            // Добавляем логику для получения ChatName и SenderName
            var chat = await _context.Chats.FindAsync(chatMessage.ChatId);
            var sender = await _context.Users.FindAsync(chatMessage.SenderId);

            ViewData["ChatName"] = chat?.Name ?? "Неизвестный чат";
            ViewData["SenderName"] = sender?.Login ?? "Неизвестный отправитель";

            return View(chatMessage);
        }

        // POST: ChatMessages/Delete/5
        // Обрабатывает подтверждение удаления сообщения чата.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chatMessage = await _context.ChatMessages.FindAsync(id);
            if (chatMessage != null)
            {
                _context.ChatMessages.Remove(chatMessage); // Удаляем сообщение из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования сообщения чата
        private bool ChatMessageExists(int id)
        {
            return _context.ChatMessages.Any(e => e.Id == id);
        }
    }
}
