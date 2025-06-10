using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models; // Убедитесь, что это пространство имен соответствует вашим моделям
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
        public async Task<IActionResult> Index()
        {
            // Соответствие: IdRating является первичным ключом,
            // ToListAsync() не требует явного указания IdRating.
            return View(await _context.Ratings.ToListAsync());
        }

        // GET: Ratings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Изменено: m.Id -> m.IdRating
            var rating = await _context.Ratings
                .FirstOrDefaultAsync(m => m.IdRating == id); // Использование IdRating как первичного ключа
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // GET: Ratings/Create
        public async Task<IActionResult> Create()
        {
            // Обновлены названия полей для SelectList на IdUser, IdPost, IdComment
            // Эти строки закомментированы, если не используются. Если используются, убедитесь, что они корректны.
            // ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login"); // Изменено "Id" на "IdUser"
            // ViewBag.IdPost = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title"); // Изменено "Id" на "IdPost"
            // ViewBag.IdComment = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content"); // Изменено "Id" на "IdComment"
            return View();
        }

        // POST: Ratings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Изменено: UserId -> IdUser, PostId -> IdPost, CommentId -> IdComment
        public async Task<IActionResult> Create([Bind("IdUser,IdPost,IdComment,Value")] Rating rating)
        {
            // Проверка на то, что только один из PostId или CommentId заполнен
            if (rating.IdPost.HasValue && rating.IdComment.HasValue) // Изменено PostId -> IdPost, CommentId -> IdComment
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария, но не для обоих.");
            }
            if (!rating.IdPost.HasValue && !rating.IdComment.HasValue) // Изменено PostId -> IdPost, CommentId -> IdComment
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(rating);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Обновлены названия полей для SelectList на IdUser, IdPost, IdComment
            // ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", rating.IdUser); // Изменено "Id" на "IdUser"
            // ViewBag.IdPost = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", rating.IdPost); // Изменено "Id" на "IdPost"
            // ViewBag.IdComment = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content", rating.IdComment); // Изменено "Id" на "IdComment"
            return View(rating);
        }

        // GET: Ratings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Изменено: FindAsync(id) будет работать, если IdRating является первичным ключом.
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }
            // Обновлены названия полей для SelectList на IdUser, IdPost, IdComment
            // ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", rating.IdUser); // Изменено "Id" на "IdUser"
            // ViewBag.IdPost = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", rating.IdPost); // Изменено "Id" на "IdPost"
            // ViewBag.IdComment = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content", rating.IdComment); // Изменено "Id" на "IdComment"
            return View(rating);
        }

        // POST: Ratings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Изменено: Id -> IdRating, UserId -> IdUser, PostId -> IdPost, CommentId -> IdComment
        public async Task<IActionResult> Edit(int id, [Bind("IdRating,IdUser,IdPost,IdComment,Value")] Rating rating) // Изменено "Id" на "IdRating"
        {
            // Изменено: id != rating.Id -> id != rating.IdRating
            if (id != rating.IdRating) // Проверяем, совпадает ли Id из маршрута с IdRating модели
            {
                return NotFound();
            }

            // Повторная проверка: оценка должна быть либо для поста, либо для комментария, но не для обоих.
            if (rating.IdPost.HasValue && rating.IdComment.HasValue) // Изменено PostId -> IdPost, CommentId -> IdComment
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария, но не для обоих.");
            }
            if (!rating.IdPost.HasValue && !rating.IdComment.HasValue) // Изменено PostId -> IdPost, CommentId -> IdComment
            {
                ModelState.AddModelError("", "Рейтинг должен быть либо для поста, либо для комментария.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Если вы хотите, чтобы UserId, PostId, CommentId не менялись при редактировании,
                    // и при этом они не приходят из формы, раскомментируйте этот блок:
                    // var existingRating = await _context.Ratings.AsNoTracking().FirstOrDefaultAsync(r => r.IdRating == id);
                    // if (existingRating == null) return NotFound();
                    // rating.IdUser = existingRating.IdUser;
                    // rating.IdPost = existingRating.IdPost;
                    // rating.IdComment = existingRating.IdComment;

                    _context.Update(rating);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Изменено: RatingExists(rating.Id) -> RatingExists(rating.IdRating)
                    if (!RatingExists(rating.IdRating)) // Использование IdRating как первичного ключа
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
            // Обновлены названия полей для SelectList на IdUser, IdPost, IdComment
            // ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", rating.IdUser);
            // ViewBag.IdPost = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", rating.IdPost);
            // ViewBag.IdComment = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content", rating.IdComment);
            return View(rating);
        }

        // GET: Ratings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Изменено: m.Id -> m.IdRating
            var rating = await _context.Ratings
                .FirstOrDefaultAsync(m => m.IdRating == id); // Использование IdRating как первичного ключа
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // POST: Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Изменено: FindAsync(id) будет работать, если IdRating является первичным ключом.
            var rating = await _context.Ratings.FindAsync(id);
            if (rating != null)
            {
                _context.Ratings.Remove(rating);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования оценки
        private bool RatingExists(int id)
        {
            // Изменено: e.Id -> e.IdRating
            return _context.Ratings.Any(e => e.IdRating == id); // Использование IdRating как первичного ключа
        }
    }
}