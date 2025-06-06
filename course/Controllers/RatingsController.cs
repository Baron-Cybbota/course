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
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ratings
        // Отображает список всех оценок.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ratings.ToListAsync());
        }

        // GET: Ratings/Details/5
        // Отображает детали конкретной оценки по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var rating = await _context.Ratings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rating == null)
            {
                return NotFound(); // Если оценка не найдена, возвращаем 404
            }

            return View(rating);
        }

        // GET: Ratings/Create
        // Отображает форму для создания новой оценки.
        public async Task<IActionResult> Create()
        {
            // Для выбора UserId, PostId и CommentId, если это требуется на форме.
            // ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login");
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title");
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content");
            return View();
        }

        // POST: Ratings/Create
        // Обрабатывает отправку формы для создания новой оценки.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // Поля, которые пользователь может ввести. Value может быть true/false.
        // PostId и CommentId являются nullable, но один из них должен быть задан.
        
        public async Task<IActionResult> Create([Bind("UserId,PostId,CommentId,Value")] Rating rating)
        {
            // Дополнительная логика проверки: оценка должна быть либо для поста, либо для комментария, но не для обоих.
            if (rating.PostId.HasValue && rating.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария, но не для обоих.");
            }
            if (!rating.PostId.HasValue && !rating.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария.");
            }

            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations и ручных проверок
            {
                _context.Add(rating); // Добавляем оценку в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список оценок
            }
            // Если модель невалидна, возвращаем форму с ошибками
            // ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", rating.UserId);
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", rating.PostId);
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content", rating.CommentId);
            return View(rating);
        }

        // GET: Ratings/Edit/5
        // Отображает форму для редактирования существующей оценки.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Ratings.FindAsync(id); // Находим оценку по Id
            if (rating == null)
            {
                return NotFound();
            }
            // Для редактирования UserId, PostId или CommentId, если они должны быть редактируемыми.
            // ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", rating.UserId);
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", rating.PostId);
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content", rating.CommentId);
            return View(rating);
        }

        // POST: Ratings/Edit/5
        // Обрабатывает отправку формы для редактирования оценки.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Убедитесь, что все поля, которые могут быть изменены пользователем, включены.
        // Id, UserId, PostId, CommentId обычно не меняются через форму редактирования после создания.
        // Здесь предполагается, что только Value может быть изменено, но Bind включает все для гибкости.
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,PostId,CommentId,Value")] Rating rating)
        {
            if (id != rating.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            // Повторная проверка: оценка должна быть либо для поста, либо для комментария, но не для обоих.
            if (rating.PostId.HasValue && rating.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария, но не для обоих.");
            }
            if (!rating.PostId.HasValue && !rating.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // В случае редактирования, если вы не хотите, чтобы PostId/CommentId менялись,
                    // можно загрузить существующую запись и перенести эти значения.
                    // var existingRating = await _context.Ratings.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    // if (existingRating == null) return NotFound();
                    // rating.UserId = existingRating.UserId;
                    // rating.PostId = existingRating.PostId;
                    // rating.CommentId = existingRating.CommentId;

                    _context.Update(rating); // Обновляем оценку в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!RatingExists(rating.Id))
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
            // ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", rating.UserId);
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", rating.PostId);
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content", rating.CommentId);
            return View(rating);
        }

        // GET: Ratings/Delete/5
        // Отображает страницу подтверждения удаления оценки.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Ratings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // POST: Ratings/Delete/5
        // Обрабатывает подтверждение удаления оценки.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating != null)
            {
                _context.Ratings.Remove(rating); // Удаляем оценку из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования оценки
        private bool RatingExists(int id)
        {
            return _context.Ratings.Any(e => e.Id == id);
        }
    }
}
