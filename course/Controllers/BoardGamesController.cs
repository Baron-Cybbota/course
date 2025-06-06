using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()

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
                .FirstOrDefaultAsync(m => m.Id == id);
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
         // Поля, которые пользователь может ввести
        public async Task<IActionResult> Create([Bind("Title,Description,ReleaseYear,MinPlayers,MaxPlayers,AveragePlayTime,Genre,Difficulty,ImageUrl")] BoardGame boardGame)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                _context.Add(boardGame); // Добавляем настольную игру в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список настольных игр
            }
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

            var boardGame = await _context.BoardGames.FindAsync(id); // Находим настольную игру по Id
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
         // Убедитесь, что все поля, которые могут быть изменены пользователем, включены
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ReleaseYear,MinPlayers,MaxPlayers,AveragePlayTime,Genre,Difficulty,ImageUrl")] BoardGame boardGame)
        {
            if (id != boardGame.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boardGame); // Обновляем настольную игру в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!BoardGameExists(boardGame.Id))
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
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var boardGame = await _context.BoardGames.FindAsync(id);
            if (boardGame != null)
            {
                _context.BoardGames.Remove(boardGame); // Удаляем настольную игру из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования настольной игры
        private bool BoardGameExists(int id)
        {
            return _context.BoardGames.Any(e => e.Id == id);
        }
    }
}
