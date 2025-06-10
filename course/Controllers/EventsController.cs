using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using course.Models.ViewModels;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace course.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();

            var boardGameIds = events.Select(e => e.IdBoardGame).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
            var locationIds = events.Select(e => e.IdLocation).Distinct().ToList();

            var boardGameNames = await _context.BoardGames
                                                 .Where(bg => boardGameIds.Contains(bg.IdBoardGame))
                                                 .ToDictionaryAsync(bg => bg.IdBoardGame, bg => bg.Name);

            var locationAddresses = await _context.Locations
                                                     .Where(l => locationIds.Contains(l.IdLocation))
                                                     .ToDictionaryAsync(l => l.IdLocation, l => l.Address);

            ViewData["BoardGameTitles"] = boardGameNames; // Changed to BoardGameTitles for consistency with view
            ViewData["LocationAddresses"] = locationAddresses;

            return View(events);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                                        .FirstOrDefaultAsync(m => m.IdEvent == id);

            if (@event == null)
            {
                return NotFound();
            }

            var boardGame = await _context.BoardGames
                                            .FirstOrDefaultAsync(bg => bg.IdBoardGame == @event.IdBoardGame);
            ViewData["BoardGameName"] = boardGame?.Name ?? "Неизвестная игра";

            var location = await _context.Locations
                                        .FirstOrDefaultAsync(l => l.IdLocation == @event.IdLocation);
            ViewData["LocationAddress"] = location?.Address ?? "Онлайн / Место не указано";

            var messages = await _context.Messages
                                            .Where(m => m.IdEvent == id)
                                            .OrderBy(m => m.SendDate)
                                            .ToListAsync();

            var senderIds = messages.Select(m => m.IdUser).Distinct().ToList();
            var senderLogins = await _context.Users
                                               .Where(u => senderIds.Contains(u.IdUser))
                                               .ToDictionaryAsync(u => u.IdUser, u => u.Login);

            var participants = await _context.EventParticipants
                                                .Where(ep => ep.IdEvent == id)
                                                .ToListAsync();

            var participantUserIds = participants.Select(ep => ep.IdUser).Distinct().ToList();
            var participantUserLogins = await _context.Users
                                                         .Where(u => participantUserIds.Contains(u.IdUser))
                                                         .ToDictionaryAsync(u => u.IdUser, u => u.Login);

            ViewData["Messages"] = messages;
            ViewData["Participants"] = participants;
            ViewData["SenderLogins"] = senderLogins;
            ViewData["ParticipantUserLogins"] = participantUserLogins;

            return View(@event);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.IdBoardGame = new SelectList(await _context.BoardGames.OrderBy(bg => bg.Name).ToListAsync(), "IdBoardGame", "Name");
            ViewBag.IdLocation = new SelectList(await _context.Locations.OrderBy(l => l.Address).ToListAsync(), "IdLocation", "Address");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBoardGame,IdLocation,Name,Date,Time,Description")] Event @event)
        {
            if (ModelState.IsValid)
            {
                @event.Date = DateTime.SpecifyKind(@event.Date, DateTimeKind.Utc);

                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Мероприятие '{@event.Name}' успешно создано.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.IdBoardGame = new SelectList(await _context.BoardGames.OrderBy(bg => bg.Name).ToListAsync(), "IdBoardGame", "Name", @event.IdBoardGame);
            ViewBag.IdLocation = new SelectList(await _context.Locations.OrderBy(l => l.Address).ToListAsync(), "IdLocation", "Address", @event.IdLocation);
            TempData["ErrorMessage"] = "Ошибка при создании мероприятия. Проверьте введенные данные.";
            return View(@event);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewBag.IdBoardGame = new SelectList(await _context.BoardGames.OrderBy(bg => bg.Name).ToListAsync(), "IdBoardGame", "Name", @event.IdBoardGame);
            ViewBag.IdLocation = new SelectList(await _context.Locations.OrderBy(l => l.Address).ToListAsync(), "IdLocation", "Address", @event.IdLocation);
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEvent,IdBoardGame,IdLocation,Name,Date,Time,Description")] Event @event)
        {
            if (id != @event.IdEvent)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    @event.Date = DateTime.SpecifyKind(@event.Date, DateTimeKind.Utc);

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Мероприятие '{@event.Name}' успешно обновлено.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.IdEvent))
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
            ViewBag.IdBoardGame = new SelectList(await _context.BoardGames.OrderBy(bg => bg.Name).ToListAsync(), "IdBoardGame", "Name", @event.IdBoardGame);
            ViewBag.IdLocation = new SelectList(await _context.Locations.OrderBy(l => l.Address).ToListAsync(), "IdLocation", "Address", @event.IdLocation);
            TempData["ErrorMessage"] = "Ошибка при обновлении мероприятия. Проверьте введенные данные.";
            return View(@event);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                                        .FirstOrDefaultAsync(m => m.IdEvent == id);

            if (@event == null)
            {
                return NotFound();
            }

            var boardGame = await _context.BoardGames
                                            .FirstOrDefaultAsync(bg => bg.IdBoardGame == @event.IdBoardGame);
            string boardGameTitle = boardGame?.Name ?? "Неизвестная игра";

            var location = await _context.Locations
                                        .FirstOrDefaultAsync(l => l.IdLocation == @event.IdLocation);
            string locationAddress = location?.Address ?? "Онлайн / Место не указано";

            var viewModel = new EventDeleteViewModel
            {
                IdEvent = @event.IdEvent,
                Name = @event.Name,
                Date = @event.Date,
                Time = @event.Time,
                Description = @event.Description,
                BoardGameTitle = boardGameTitle,
                LocationAddress = locationAddress
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Мероприятие успешно удалено.";
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.IdEvent == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int idEvent, string content, int idUser)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Сообщение не может быть пустым.";
                return RedirectToAction(nameof(Details), new { id = idEvent });
            }

            var eventExists = await _context.Events.AnyAsync(e => e.IdEvent == idEvent);
            var senderExists = await _context.Users.AnyAsync(u => u.IdUser == idUser);

            if (!eventExists || !senderExists)
            {
                TempData["ErrorMessage"] = "Не удалось отправить сообщение: мероприятие или отправитель не существуют.";
                return RedirectToAction(nameof(Details), new { id = idEvent });
            }

            var message = new Message
            {
                IdEvent = idEvent,
                IdUser = idUser,
                Content = content,
                SendDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Сообщение отправлено.";
            return RedirectToAction(nameof(Details), new { id = idEvent });
        }

        public async Task<IActionResult> LoadMessages(int idEvent)
        {
            var messages = await _context.Messages
                                            .Where(m => m.IdEvent == idEvent)
                                            .OrderBy(m => m.SendDate)
                                            .ToListAsync();

            var senderIds = messages.Select(m => m.IdUser).Distinct().ToList();

            var senderLogins = await _context.Users
                                               .Where(u => senderIds.Contains(u.IdUser))
                                               .ToDictionaryAsync(u => u.IdUser, u => u.Login);

            var messagesWithSenderLogin = messages.Select(m => new {
                m.IdMessage,
                m.Content,
                m.SendDate,
                SenderLogin = senderLogins.TryGetValue(m.IdUser, out var login) ? login : "Неизвестный"
            }).ToList();

            return Json(messagesWithSenderLogin);
        }
    }
}