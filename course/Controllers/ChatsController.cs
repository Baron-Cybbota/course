using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq;

namespace course.Controllers
{
    [Authorize] // Все действия в этом контроллере требуют авторизации
    public class ChatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChatsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: Chats (Index)
        //Отображает список всех чатов. В реальном приложении, возможно, вы бы показывали только чаты, в которых участвует пользователь.
        public async Task<IActionResult> Index()
        {
            // Получаем список всех чатов
            var chats = await _context.Chats.ToListAsync();

            // Если нужно отобразить имя создателя чата в списке:
            // 1. Собираем все уникальные CreatorId из полученных чатов.
            var creatorIds = chats.Select(c => c.CreatorId).Distinct().ToList();
            // 2. Загружаем всех соответствующих пользователей.
            var creators = await _userManager.Users.Where(u => creatorIds.Contains(u.Id)).ToListAsync();
            // 3. Создаем словарь для быстрого поиска имени пользователя по ID в представлении.
            ViewData["Creators"] = creators.ToDictionary(u => u.Id, u => u.UserName);

            return View(chats);
        }

        // GET: Chats/Details/5
        //Отображает детали конкретного чата.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(m => m.Id == id);
            if (chat == null)
            {
                return NotFound();
            }

            // Загружаем имя создателя чата
            var creator = await _userManager.FindByIdAsync(chat.CreatorId.ToString());
            ViewData["CreatorName"] = creator?.UserName;

            // --- Добавлены строки для загрузки сообщений и отправителей ---
            var messages = await _context.Messages
                .Where(m => m.ChatId == chat.Id)
                .OrderBy(m => m.SentDate)
                .ToListAsync();
            ViewData["Messages"] = messages;

            // Собираем уникальные ID отправителей сообщений
            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();

            // Загружаем всех соответствующих отправителей
            var senders = await _userManager.Users.Where(u => senderIds.Contains(u.Id)).ToListAsync();

            // Создаем словарь для быстрого поиска имени отправителя по ID
            ViewData["MessageSenders"] = senders.ToDictionary(u => u.Id, u => u.UserName);
            // --- Конец добавленных строк ---

            return View(chat);
        }

        // GET: Chats/Create
        //Отображает форму для создания нового чата.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chats/Create
        //Обрабатывает отправку формы создания нового чата.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Chat chat)
        {
            // Получаем ID текущего пользователя из ClaimTypes.NameIdentifier
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                ModelState.AddModelError(string.Empty, "Вы должны быть авторизованы для создания чата.");
                return View(chat);
            }

            chat.CreatorId = int.Parse(userIdString);
            chat.CreatedDate = DateTime.UtcNow; // Устанавливаем текущую дату создания

            if (ModelState.IsValid)
            {
                _context.Add(chat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chat);
        }

        // GET: Chats/Edit/5
        //Отображает форму для редактирования чата.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats.FindAsync(id);
            if (chat == null)
            {
                return NotFound();
            }

            // Проверяем, является ли текущий пользователь создателем чата или модератором
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (chat.CreatorId != int.Parse(currentUserId) && !User.IsInRole("Модератор"))
            {
                return Forbid(); // Запрещаем доступ
            }

            return View(chat);
        }

        // POST: Chats/Edit/5
        //Обрабатывает отправку формы редактирования чата.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Chat chat) // CreatorId и CreatedDate не должны меняться из формы
        {
            if (id != chat.Id)
            {
                return NotFound();
            }

            // Получаем существующий чат для сохранения неизменяемых полей (CreatorId, CreatedDate)
            var existingChat = await _context.Chats.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingChat == null)
            {
                return NotFound();
            }

            // Повторная проверка прав доступа
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingChat.CreatorId != int.Parse(currentUserId) && !User.IsInRole("Модератор"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Восстанавливаем неизменяемые поля из существующего чата
                    chat.CreatorId = existingChat.CreatorId;
                    chat.CreatedDate = existingChat.CreatedDate;

                    _context.Update(chat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChatExists(chat.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chat);
        }

        // GET: Chats/Delete/5
        //Отображает страницу подтверждения удаления чата.
        [Authorize(Roles = "Модератор")] // Только модераторы могут удалять чаты (или создатель)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(m => m.Id == id);
            if (chat == null)
            {
                return NotFound();
            }

            // Загружаем имя создателя для отображения в представлении
            var creator = await _userManager.FindByIdAsync(chat.CreatorId.ToString());
            ViewData["CreatorName"] = creator?.UserName;

            // Если вы хотите разрешить создателю удалять свой чат, раскомментируйте:
            // var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (chat.CreatorId != int.Parse(currentUserId) && !User.IsInRole("Модератор"))
            // {
            //     return Forbid();
            // }

            return View(chat);
        }

        // POST: Chats/Delete/5
        //Обрабатывает подтверждение удаления чата.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Модератор")] // Только модераторы могут подтвердить удаление
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat != null)
            {
                // Проверка прав также важна здесь на случай прямого POST-запроса
                // var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // if (chat.CreatorId != int.Parse(currentUserId) && !User.IsInRole("Модератор"))
                // {
                //     return Forbid(); // Если вдруг кто-то обошел GET-запрос
                // }

                _context.Chats.Remove(chat);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод ChatExists
        private bool ChatExists(int id)
        {
            return _context.Chats.Any(e => e.Id == id);
        }
    }
}