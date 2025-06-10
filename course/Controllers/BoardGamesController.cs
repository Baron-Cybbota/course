using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
// No SelectList is needed as there are no foreign keys to populate dropdowns directly in this controller.

namespace course.Controllers
{
    public class BoardGamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoardGamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BoardGames
        // Отображает список всех настольных игр.
        public async Task<IActionResult> Index()
        {
            return View(await _context.BoardGames.ToListAsync());
        }

        // GET: BoardGames/Details/5
        // Отображает детали конкретной настольной игры по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var boardGame = await _context.BoardGames
                .FirstOrDefaultAsync(m => m.IdBoardGame == id); // CORRECTED: Use IdBoardGame
            if (boardGame == null)
            {
                return NotFound(); // Если настольная игра не найдена, возвращаем 404
            }

            return View(boardGame);
        }

        // GET: BoardGames/Create
        // Отображает форму для создания новой настольной игры.
        public IActionResult Create()
        {
            return View();
        }

        // POST: BoardGames/Create
        // Обрабатывает отправку формы для создания новой настольной игры.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // CORRECTED: Use Name and EstimatedPlayTime, removed ImageUrl as it's not on BoardGame model
        public async Task<IActionResult> Create([Bind("Name,Description,ReleaseYear,MinPlayers,MaxPlayers,EstimatedPlayTime,Genre,Difficulty")] BoardGame boardGame)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                _context.Add(boardGame); // Добавляем настольную игру в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                TempData["SuccessMessage"] = "Настольная игра успешно добавлена.";
                return RedirectToAction(nameof(Index)); // Перенаправляем на список настольных игр
            }
            TempData["ErrorMessage"] = "Ошибка при создании настольной игры. Проверьте введенные данные.";
            return View(boardGame); // Если модель невалидна, возвращаем форму с ошибками
        }

        // GET: BoardGames/Edit/5
        // Отображает форму для редактирования существующей настольной игры.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardGame = await _context.BoardGames.FindAsync(id); // Will implicitly use IdBoardGame
            if (boardGame == null)
            {
                return NotFound();
            }
            return View(boardGame);
        }

        // POST: BoardGames/Edit/5
        // Обрабатывает отправку формы для редактирования настольной игры.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CORRECTED: Use IdBoardGame in Bind, and Name, EstimatedPlayTime. Removed ImageUrl.
        public async Task<IActionResult> Edit(int id, [Bind("IdBoardGame,Name,Description,ReleaseYear,MinPlayers,MaxPlayers,EstimatedPlayTime,Genre,Difficulty")] BoardGame boardGame)
        {
            if (id != boardGame.IdBoardGame) // CORRECTED: Check against IdBoardGame
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // No need to use AsNoTracking and re-assign if all properties are bound and expected to change.
                    // If you had properties not in the form and not in Bind, you would fetch and preserve them here.
                    _context.Update(boardGame); // Обновляем настольную игру в контексте
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Настольная игра успешно обновлена.";
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!BoardGameExists(boardGame.IdBoardGame)) // CORRECTED: Check against IdBoardGame
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
            TempData["ErrorMessage"] = "Ошибка при обновлении настольной игры. Проверьте введенные данные.";
            return View(boardGame);
        }

        // GET: BoardGames/Delete/5
        // Отображает страницу подтверждения удаления настольной игры.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardGame = await _context.BoardGames
                .FirstOrDefaultAsync(m => m.IdBoardGame == id); // CORRECTED: Use IdBoardGame
            if (boardGame == null)
            {
                return NotFound();
            }

            return View(boardGame);
        }

        // POST: BoardGames/Delete/5
        // Обрабатывает подтверждение удаления настольной игры.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var boardGame = await _context.BoardGames.FindAsync(id); // Will implicitly use IdBoardGame
            if (boardGame != null)
            {
                _context.BoardGames.Remove(boardGame); // Удаляем настольную игру из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            TempData["SuccessMessage"] = "Настольная игра успешно удалена.";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования настольной игры
        private bool BoardGameExists(int id)
        {
            return _context.BoardGames.Any(e => e.IdBoardGame == id); // CORRECTED: Use IdBoardGame
        }
    }
}