using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
using System.Collections.Generic; // Для Dictionary
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList

namespace course.Controllers
{
    public class ChatParticipantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatParticipantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ChatParticipants
        // Отображает список всех участников чатов.
        public async Task<IActionResult> Index()
        {
            var participants = await _context.ChatParticipants.ToListAsync();

            // Получаем все уникальные ChatId из участников
            var chatIds = participants.Select(p => p.ChatId).Distinct().ToList();
            // Получаем все уникальные UserId из участников
            var userIds = participants.Select(p => p.UserId).Distinct().ToList();

            // Загружаем названия чатов
            var chats = await _context.Chats
                                      .Where(c => chatIds.Contains(c.Id))
                                      .ToDictionaryAsync(c => c.Id, c => c.Name);

            // Загружаем логины пользователей
            var users = await _context.Users
                                     .Where(u => userIds.Contains(u.Id))
                                     .ToDictionaryAsync(u => u.Id, u => u.Login);

            ViewData["ChatNames"] = chats;
            ViewData["UserNames"] = users;

            return View(participants);
        }

        // GET: ChatParticipants/Details?chatId=1&userId=101
        // Отображает детали конкретного участника чата по составному ключу (ChatId и UserId).
        public async Task<IActionResult> Details(int? chatId, int? userId)
        {
            if (chatId == null || userId == null)
            {
                return NotFound(); // Если один из ключей не предоставлен, возвращаем 404
            }

            var participant = await _context.ChatParticipants
                .FirstOrDefaultAsync(m => m.ChatId == chatId && m.UserId == userId);
            if (participant == null)
            {
                return NotFound(); // Если участник не найден, возвращаем 404
            }

            // Для отображения имен чатов и пользователей в деталях
            var chat = await _context.Chats.FindAsync(participant.ChatId);
            var user = await _context.Users.FindAsync(participant.UserId);

            ViewData["ChatName"] = chat?.Name ?? "Неизвестный чат";
            ViewData["UserName"] = user?.Login ?? "Неизвестный пользователь";

            return View(participant);
        }

        // GET: ChatParticipants/Create
        // Отображает форму для добавления нового участника в чат.
        public async Task<IActionResult> Create()
        {
            ViewBag.Chats = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name");
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login");
            return View();
        }

        // POST: ChatParticipants/Create
        // Обрабатывает отправку формы для создания нового участника чата.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // Только эти поля, JoinDate будет установлена сервером
        public async Task<IActionResult> Create([Bind("ChatId,UserId")] ChatParticipant chatParticipant)
        {
            // Проверяем, не является ли пользователь уже участником этого чата.
            // Это предотвратит ошибку составного уникального ключа.
            if (await _context.ChatParticipants.AnyAsync(cp => cp.ChatId == chatParticipant.ChatId && cp.UserId == chatParticipant.UserId))
            {
                ModelState.AddModelError("", "Этот пользователь уже является участником данного чата.");
                ViewBag.Chats = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name", chatParticipant.ChatId);
                ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chatParticipant.UserId);
                return View(chatParticipant);
            }

            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                chatParticipant.JoinDate = DateTime.UtcNow; // Устанавливаем дату присоединения
                
                _context.Add(chatParticipant); // Добавляем участника в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список участников
            }
            // Если модель невалидна, возвращаем форму с ошибками
            ViewBag.Chats = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name", chatParticipant.ChatId);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chatParticipant.UserId);
            return View(chatParticipant);
        }

        // GET: ChatParticipants/Edit?chatId=1&userId=101
        // Отображает форму для редактирования существующего участника чата.
        // Обратите внимание: два параметра для составного ключа.
        public async Task<IActionResult> Edit(int? chatId, int? userId)
        {
            if (chatId == null || userId == null)
            {
                return NotFound();
            }

            // Используем FindAsync с составным ключом (важно: порядок параметров должен совпадать с определением ключа в DbContext)
            var participant = await _context.ChatParticipants.FindAsync(chatId, userId); 
            if (participant == null)
            {
                return NotFound();
            }
            // Для редактирования, возможно, вы захотите дать выбрать другие чат или пользователя,
            // но обычно ключевые поля составного ключа не меняются при редактировании.
            // Если вы хотите, чтобы эти поля были редактируемыми, вам нужно будет обрабатывать уникальность
            // и возможные удаления/создания записей, если ключ меняется.
            ViewBag.Chats = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name", participant.ChatId);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", participant.UserId);
            return View(participant);
        }

        // POST: ChatParticipants/Edit?chatId=1&userId=101
        // Обрабатывает отправку формы для редактирования участника чата.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind("ChatId,UserId,JoinDate") - убедитесь, что все поля, которые могут быть изменены, включены.
        // Если вы не позволяете менять ChatId или UserId, их можно убрать из Bind.
        public async Task<IActionResult> Edit(int chatId, int userId, [Bind("ChatId,UserId,JoinDate")] ChatParticipant chatParticipant)
        {
            // Проверяем, что переданные Id из маршрута соответствуют ключевым полям модели
            if (chatId != chatParticipant.ChatId || userId != chatParticipant.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chatParticipant); // Обновляем участника в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!ChatParticipantExists(chatParticipant.ChatId, chatParticipant.UserId))
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
            ViewBag.Chats = new SelectList(await _context.Chats.ToListAsync(), "Id", "Name", chatParticipant.ChatId);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chatParticipant.UserId);
            return View(chatParticipant);
        }

        // GET: ChatParticipants/Delete?chatId=1&userId=101
        // Отображает страницу подтверждения удаления участника чата.
        public async Task<IActionResult> Delete(int? chatId, int? userId)
        {
            if (chatId == null || userId == null)
            {
                return NotFound();
            }

            var participant = await _context.ChatParticipants
                .FirstOrDefaultAsync(m => m.ChatId == chatId && m.UserId == userId);
            if (participant == null)
            {
                return NotFound();
            }

            // Для отображения имен чатов и пользователей в деталях
            var chat = await _context.Chats.FindAsync(participant.ChatId);
            var user = await _context.Users.FindAsync(participant.UserId);

            ViewData["ChatName"] = chat?.Name ?? "Неизвестный чат";
            ViewData["UserName"] = user?.Login ?? "Неизвестный пользователь";

            return View(participant);
        }

        // POST: ChatParticipants/DeleteConfirmed
        // Обрабатывает подтверждение удаления участника чата.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int chatId, int userId)
        {
            // Находим участника по составному ключу
            var participant = await _context.ChatParticipants.FindAsync(chatId, userId);
            if (participant != null)
            {
                _context.ChatParticipants.Remove(participant); // Удаляем участника из контекста
            }
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования участника чата по составному ключу
        private bool ChatParticipantExists(int chatId, int userId)
        {
            return _context.ChatParticipants.Any(e => e.ChatId == chatId && e.UserId == userId);
        }
    }
}
