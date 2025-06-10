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

            // Получаем все уникальные IdEvent из участников
            var eventIds = participants.Select(p => p.IdEvent).Distinct().ToList(); // Corrected from p.EventId to p.IdEvent
            // Получаем все уникальные IdUser из участников
            var userIds = participants.Select(p => p.IdUser).Distinct().ToList();   // Corrected from p.UserId to p.IdUser

            // Загружаем названия мероприятий
            var eventNames = await _context.Events
                                             .Where(e => eventIds.Contains(e.IdEvent)) // Corrected from e.Id to e.IdEvent
                                             .ToDictionaryAsync(e => e.IdEvent, e => e.Name); // Corrected from e.Id to e.IdEvent and e.Description to e.Name (assuming Name is the display property)

            // Загружаем логины пользователей
            var userLogins = await _context.Users
                                           .Where(u => userIds.Contains(u.IdUser)) // Corrected from u.Id to u.IdUser
                                           .ToDictionaryAsync(u => u.IdUser, u => u.Login); // Corrected from u.Id to u.IdUser

            ViewData["EventNames"] = eventNames; // Renamed to EventNames for consistency
            ViewData["UserLogins"] = userLogins;

            return View(participants);
        }

        // GET: EventParticipants/Details?eventId=1&userId=101
        // Отображает детали конкретного участника мероприятия по составному ключу (IdEvent и IdUser).
        public async Task<IActionResult> Details(int? idEvent, int? idUser) // Corrected parameter names
        {
            if (idEvent == null || idUser == null) // Corrected parameter names
            {
                return NotFound(); // Если один из ключей не предоставлен, возвращаем 404
            }

            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(m => m.IdEvent == idEvent && m.IdUser == idUser); // Corrected to IdEvent and IdUser
            if (participant == null)
            {
                return NotFound(); // Если участник не найден, возвращаем 404
            }

            // Получаем название связанного мероприятия и логин пользователя
            // Using FirstOrDefaultAsync to fetch, as FindAsync expects primary key only
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.IdEvent == participant.IdEvent); // Corrected to IdEvent
            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == participant.IdUser); // Corrected to IdUser

            ViewData["EventName"] = eventEntity?.Name ?? "Неизвестное мероприятие"; // Corrected from Description to Name
            ViewData["UserLogin"] = user?.Login ?? "Неизвестный пользователь";

            return View(participant);
        }

        // GET: EventParticipants/Create
        // Отображает форму для добавления нового участника в мероприятие.
        public async Task<IActionResult> Create()
        {
            // Для выбора IdEvent и IdUser, передаем списки доступных мероприятий и пользователей.
            // Using IdEvent and Name for Events, IdUser and Login for Users
            ViewBag.Events = new SelectList(await _context.Events.OrderBy(e => e.Name).ToListAsync(), "IdEvent", "Name"); // Corrected "Id" to "IdEvent", "Description" to "Name"
            ViewBag.Users = new SelectList(await _context.Users.OrderBy(u => u.Login).ToListAsync(), "IdUser", "Login"); // Corrected "Id" to "IdUser"
            return View();
        }

        // POST: EventParticipants/Create
        // Обрабатывает отправку формы для создания нового участника мероприятия.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // Только эти поля
        public async Task<IActionResult> Create([Bind("IdEvent,IdUser,ParticipationStatus")] EventParticipant eventParticipant) // Corrected Bind properties to IdEvent, IdUser
        {
            // Проверяем, не является ли пользователь уже участником этого мероприятия.
            // Это предотвратит ошибку составного уникального ключа.
            if (await _context.EventParticipants.AnyAsync(ep => ep.IdEvent == eventParticipant.IdEvent && ep.IdUser == eventParticipant.IdUser)) // Corrected to IdEvent and IdUser
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
            ViewBag.Events = new SelectList(await _context.Events.OrderBy(e => e.Name).ToListAsync(), "IdEvent", "Name", eventParticipant.IdEvent); // Corrected
            ViewBag.Users = new SelectList(await _context.Users.OrderBy(u => u.Login).ToListAsync(), "IdUser", "Login", eventParticipant.IdUser); // Corrected
            return View(eventParticipant);
        }

        // GET: EventParticipants/Edit?idEvent=1&idUser=101
        // Отображает форму для редактирования существующего участника мероприятия.
        // Обратите внимание: два параметра для составного ключа.
        public async Task<IActionResult> Edit(int? idEvent, int? idUser) // Corrected parameter names
        {
            if (idEvent == null || idUser == null) // Corrected parameter names
            {
                return NotFound();
            }

            // Используем FindAsync с составным ключом (важно: порядок параметров должен совпадать с определением ключа в DbContext)
            var participant = await _context.EventParticipants.FindAsync(idEvent, idUser); // Corrected parameter names
            if (participant == null)
            {
                return NotFound();
            }
            // Обычно ключевые поля составного ключа не меняются при редактировании.
            // Если вы хотите, чтобы эти поля были редактируемыми, вам нужно будет обрабатывать уникальность
            // и возможные удаления/создания записей, если ключ меняется.
            // При редактировании, пользователь и мероприятие уже выбраны, так что эти SelectList могут быть не нужны
            // или использоваться для отображения текущих значений.
            ViewBag.Events = new SelectList(await _context.Events.OrderBy(e => e.Name).ToListAsync(), "IdEvent", "Name", participant.IdEvent); // Corrected
            ViewBag.Users = new SelectList(await _context.Users.OrderBy(u => u.Login).ToListAsync(), "IdUser", "Login", participant.IdUser); // Corrected
            return View(participant);
        }

        // POST: EventParticipants/Edit?idEvent=1&idUser=101
        // Обрабатывает отправку формы для редактирования участника мероприятия.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int idEvent, int idUser, [Bind("IdEvent,IdUser,ParticipationStatus")] EventParticipant eventParticipant) // Corrected parameter names and Bind properties
        {
            // Проверяем, что переданные Id из маршрута соответствуют ключевым полям модели
            if (idEvent != eventParticipant.IdEvent || idUser != eventParticipant.IdUser) // Corrected to IdEvent and IdUser
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
                    if (!EventParticipantExists(eventParticipant.IdEvent, eventParticipant.IdUser)) // Corrected to IdEvent and IdUser
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
            ViewBag.Events = new SelectList(await _context.Events.OrderBy(e => e.Name).ToListAsync(), "IdEvent", "Name", eventParticipant.IdEvent); // Corrected
            ViewBag.Users = new SelectList(await _context.Users.OrderBy(u => u.Login).ToListAsync(), "IdUser", "Login", eventParticipant.IdUser); // Corrected
            return View(eventParticipant);
        }

        // GET: EventParticipants/Delete?idEvent=1&idUser=101
        // Отображает страницу подтверждения удаления участника мероприятия.
        public async Task<IActionResult> Delete(int? idEvent, int? idUser) // Corrected parameter names
        {
            if (idEvent == null || idUser == null) // Corrected parameter names
            {
                return NotFound();
            }

            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(m => m.IdEvent == idEvent && m.IdUser == idUser); // Corrected to IdEvent and IdUser
            if (participant == null)
            {
                return NotFound();
            }

            // Получаем название связанного мероприятия и логин пользователя
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.IdEvent == participant.IdEvent); // Corrected to IdEvent
            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == participant.IdUser); // Corrected to IdUser

            ViewData["EventName"] = eventEntity?.Name ?? "Неизвестное мероприятие"; // Corrected to Name
            ViewData["UserLogin"] = user?.Login ?? "Неизвестный пользователь";

            return View(participant);
        }

        // POST: EventParticipants/DeleteConfirmed
        // Обрабатывает подтверждение удаления участника мероприятия.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int idEvent, int idUser) // Corrected parameter names
        {
            // Находим участника по составному ключу
            var participant = await _context.EventParticipants.FindAsync(idEvent, idUser); // Corrected parameter names
            if (participant != null)
            {
                _context.EventParticipants.Remove(participant); // Удаляем участника из контекста
            }
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования участника мероприятия по составному ключу
        private bool EventParticipantExists(int idEvent, int idUser) // Corrected parameter names
        {
            return _context.EventParticipants.Any(e => e.IdEvent == idEvent && e.IdUser == idUser); // Corrected to IdEvent and IdUser
        }
    }
}