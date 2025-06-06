using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList
using System.Collections.Generic; // Для Dictionary

namespace course.Controllers
{
    public class EventParticipantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventParticipantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EventParticipants
        // Отображает список всех участников мероприятий.
        public async Task<IActionResult> Index()
        {
            var participants = await _context.EventParticipants.ToListAsync();

            // Получаем все уникальные EventId из участников
            var eventIds = participants.Select(p => p.EventId).Distinct().ToList();
            // Получаем все уникальные UserId из участников
            var userIds = participants.Select(p => p.UserId).Distinct().ToList();

            // Загружаем названия мероприятий
            var eventTitles = await _context.Events
                                            .Where(e => eventIds.Contains(e.Id))
                                            .ToDictionaryAsync(e => e.Id, e => e.Description); // Или 'Title', если есть

            // Загружаем логины пользователей
            var userLogins = await _context.Users
                                          .Where(u => userIds.Contains(u.Id))
                                          .ToDictionaryAsync(u => u.Id, u => u.Login);

            ViewData["EventTitles"] = eventTitles;
            ViewData["UserLogins"] = userLogins;

            return View(participants);
        }

        // GET: EventParticipants/Details?eventId=1&userId=101
        // Отображает детали конкретного участника мероприятия по составному ключу (EventId и UserId).
        public async Task<IActionResult> Details(int? eventId, int? userId)
        {
            if (eventId == null || userId == null)
            {
                return NotFound(); // Если один из ключей не предоставлен, возвращаем 404
            }

            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(m => m.EventId == eventId && m.UserId == userId);
            if (participant == null)
            {
                return NotFound(); // Если участник не найден, возвращаем 404
            }

            // Получаем название связанного мероприятия и логин пользователя
            var eventEntity = await _context.Events.FindAsync(participant.EventId);
            var user = await _context.Users.FindAsync(participant.UserId);

            ViewData["EventTitle"] = eventEntity?.Description ?? "Неизвестное мероприятие"; // Или 'Title', если есть
            ViewData["UserLogin"] = user?.Login ?? "Неизвестный пользователь";

            return View(participant);
        }

        // GET: EventParticipants/Create
        // Отображает форму для добавления нового участника в мероприятие.
        public async Task<IActionResult> Create()
        {
            // Для выбора EventId и UserId, передаем списки доступных мероприятий и пользователей.
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "Id", "Description"); // Или "Title" для мероприятий, если есть
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login");
            return View();
        }

        // POST: EventParticipants/Create
        // Обрабатывает отправку формы для создания нового участника мероприятия.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // Только эти поля
        public async Task<IActionResult> Create([Bind("EventId,UserId,ParticipationStatus")] EventParticipant eventParticipant)
        {
            // Проверяем, не является ли пользователь уже участником этого мероприятия.
            // Это предотвратит ошибку составного уникального ключа.
            if (await _context.EventParticipants.AnyAsync(ep => ep.EventId == eventParticipant.EventId && ep.UserId == eventParticipant.UserId))
            {
                ModelState.AddModelError("", "Этот пользователь уже является участником данного мероприятия.");
            }

            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                _context.Add(eventParticipant); // Добавляем участника в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список участников
            }
            // Если модель невалидна, возвращаем форму с ошибками, повторно заполняя ViewBag
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "Id", "Description", eventParticipant.EventId);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", eventParticipant.UserId);
            return View(eventParticipant);
        }

        // GET: EventParticipants/Edit?eventId=1&userId=101
        // Отображает форму для редактирования существующего участника мероприятия.
        // Обратите внимание: два параметра для составного ключа.
        public async Task<IActionResult> Edit(int? eventId, int? userId)
        {
            if (eventId == null || userId == null)
            {
                return NotFound();
            }

            // Используем FindAsync с составным ключом (важно: порядок параметров должен совпадать с определением ключа в DbContext)
            var participant = await _context.EventParticipants.FindAsync(eventId, userId); 
            if (participant == null)
            {
                return NotFound();
            }
            // Обычно ключевые поля составного ключа не меняются при редактировании.
            // Если вы хотите, чтобы эти поля были редактируемыми, вам нужно будет обрабатывать уникальность
            // и возможные удаления/создания записей, если ключ меняется.
            // При редактировании, пользователь и мероприятие уже выбраны, так что эти SelectList могут быть не нужны
            // или использоваться для отображения текущих значений.
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "Id", "Description", participant.EventId);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", participant.UserId);
            return View(participant);
        }

        // POST: EventParticipants/Edit?eventId=1&userId=101
        // Обрабатывает отправку формы для редактирования участника мероприятия.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int eventId, int userId, [Bind("EventId,UserId,ParticipationStatus")] EventParticipant eventParticipant)
        {
            // Проверяем, что переданные Id из маршрута соответствуют ключевым полям модели
            if (eventId != eventParticipant.EventId || userId != eventParticipant.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventParticipant); // Обновляем участника в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!EventParticipantExists(eventParticipant.EventId, eventParticipant.UserId))
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
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "Id", "Description", eventParticipant.EventId);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", eventParticipant.UserId);
            return View(eventParticipant);
        }

        // GET: EventParticipants/Delete?eventId=1&userId=101
        // Отображает страницу подтверждения удаления участника мероприятия.
        public async Task<IActionResult> Delete(int? eventId, int? userId)
        {
            if (eventId == null || userId == null)
            {
                return NotFound();
            }

            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(m => m.EventId == eventId && m.UserId == userId);
            if (participant == null)
            {
                return NotFound();
            }

            // Получаем название связанного мероприятия и логин пользователя
            var eventEntity = await _context.Events.FindAsync(participant.EventId);
            var user = await _context.Users.FindAsync(participant.UserId);

            ViewData["EventTitle"] = eventEntity?.Description ?? "Неизвестное мероприятие";
            ViewData["UserLogin"] = user?.Login ?? "Неизвестный пользователь";

            return View(participant);
        }

        // POST: EventParticipants/DeleteConfirmed
        // Обрабатывает подтверждение удаления участника мероприятия.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int eventId, int userId)
        {
            // Находим участника по составному ключу
            var participant = await _context.EventParticipants.FindAsync(eventId, userId);
            if (participant != null)
            {
                _context.EventParticipants.Remove(participant); // Удаляем участника из контекста
            }
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования участника мероприятия по составному ключу
        private bool EventParticipantExists(int eventId, int userId)
        {
            return _context.EventParticipants.Any(e => e.EventId == eventId && e.UserId == userId);
        }
    }
}
