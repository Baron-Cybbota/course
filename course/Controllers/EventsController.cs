using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime и TimeSpan
using System.Linq; // Для Any()
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList
using System.Collections.Generic; // Для Dictionary

namespace course.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        // Отображает список всех мероприятий.
        public async Task<IActionResult> Index()
        {
            // Загружаем мероприятия и связанные настольные игры
            var events = await _context.Events.ToListAsync();
            var boardGameIds = events.Select(e => e.BoardGameId).Distinct().ToList();
            var boardGameTitles = await _context.BoardGames
                                                .Where(bg => boardGameIds.Contains(bg.Id))
                                                .ToDictionaryAsync(bg => bg.Id, bg => bg.Title);

            ViewData["BoardGameTitles"] = boardGameTitles;
            return View(events);
        }

        // GET: Events/Details/5
        // Отображает детали конкретного мероприятия по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound(); // Если мероприятие не найдено, возвращаем 404
            }

            // Получаем название связанной настольной игры
            var boardGame = await _context.BoardGames.FindAsync(@event.BoardGameId);
            ViewData["BoardGameTitle"] = boardGame?.Title ?? "Неизвестная игра";

            return View(@event);
        }

        // GET: Events/Create
        // Отображает форму для создания нового мероприятия.
        public async Task<IActionResult> Create()
        {
            // Для выбора BoardGameId, передаем список доступных настольных игр.
            ViewBag.BoardGameId = new SelectList(await _context.BoardGames.ToListAsync(), "Id", "Title");
            return View();
        }

        // POST: Events/Create
        // Обрабатывает отправку формы для создания нового мероприятия.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        public async Task<IActionResult> Create([Bind("BoardGameId,Date,Time,Location,Description")] Event @event)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                // ИСПРАВЛЕНИЕ: Устанавливаем Kind для DateTime на Utc перед сохранением
                @event.Date = DateTime.SpecifyKind(@event.Date, DateTimeKind.Utc);
                
                _context.Add(@event); // Добавляем мероприятие в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список мероприятий
            }
            // Если модель невалидна, возвращаем форму с ошибками, повторно заполняя ViewBag
            ViewBag.BoardGameId = new SelectList(await _context.BoardGames.ToListAsync(), "Id", "Title", @event.BoardGameId);
            return View(@event);
        }

        // GET: Events/Edit/5
        // Отображает форму для редактирования существующего мероприятия.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id); // Находим мероприятие по Id
            if (@event == null)
            {
                return NotFound();
            }
            // Для редактирования BoardGameId, повторно заполняем ViewBag
            ViewBag.BoardGameId = new SelectList(await _context.BoardGames.ToListAsync(), "Id", "Title", @event.BoardGameId);
            return View(@event);
        }

        // POST: Events/Edit/5
        // Обрабатывает отправку формы для редактирования мероприятия.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BoardGameId,Date,Time,Location,Description")] Event @event)
        {
            if (id != @event.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ИСПРАВЛЕНИЕ: Устанавливаем Kind для DateTime на Utc перед сохранением
                    @event.Date = DateTime.SpecifyKind(@event.Date, DateTimeKind.Utc);

                    _context.Update(@event); // Обновляем мероприятие в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!EventExists(@event.Id))
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
            // Если модель невалидна, возвращаем форму с ошибками, повторно заполняя ViewBag
            ViewBag.BoardGameId = new SelectList(await _context.BoardGames.ToListAsync(), "Id", "Title", @event.BoardGameId);
            return View(@event);
        }

        // GET: Events/Delete/5
        // Отображает страницу подтверждения удаления мероприятия.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            // Получаем название связанной настольной игры
            var boardGame = await _context.BoardGames.FindAsync(@event.BoardGameId);
            ViewData["BoardGameTitle"] = boardGame?.Title ?? "Неизвестная игра";

            return View(@event);
        }

        // POST: Events/Delete/5
        // Обрабатывает подтверждение удаления мероприятия.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event); // Удаляем мероприятие из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования мероприятия
        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
