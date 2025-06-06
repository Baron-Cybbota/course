using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
using System.Collections.Generic; // Для Dictionary
using System.Security.Claims; // Для работы с Claims

namespace course.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posts
        // Отображает список всех постов.
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts.ToListAsync();

            // Получаем уникальные AuthorId из всех постов
            var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();

            // Загружаем авторов для этих ID
            var authors = await _context.Users
                                        .Where(u => authorIds.Contains(u.Id))
                                        .ToDictionaryAsync(u => u.Id, u => u.Login);

            ViewData["Authors"] = authors; // Передаем словарь авторов в View

            return View(posts);
        }

        // GET: Posts/Details/5
        // Отображает детали конкретного поста по Id, включая связанные данные.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound(); // Если пост не найден, возвращаем 404
            }

            // Получаем имя автора поста
            var author = await _context.Users.FindAsync(post.AuthorId);
            ViewData["AuthorName"] = author?.Login ?? "Неизвестный автор";

            // Получаем все комментарии к этому посту
            var comments = await _context.Comments
                                         .Where(c => c.PostId == id)
                                         .OrderBy(c => c.CreationDate)
                                         .ToListAsync();
            ViewData["Comments"] = comments;

            // Получаем имена авторов комментариев
            var commentAuthorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
            var commentAuthors = await _context.Users
                                               .Where(u => commentAuthorIds.Contains(u.Id))
                                               .ToDictionaryAsync(u => u.Id, u => u.Login);
            ViewData["CommentAuthors"] = commentAuthors;

            // Получаем рейтинги для этого поста
            var postRatings = await _context.Ratings.Where(r => r.PostId == id).ToListAsync();

            // Вычисляем общий рейтинг поста
            int totalPostRating = postRatings.Sum(r => r.Value ? 1 : -1);
            ViewData["TotalPostRating"] = totalPostRating;
            // Передаем количество оценок для поста
            ViewData["PostRatingsCount"] = postRatings.Count;

            // Получаем текущего пользователя для логики оценки
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }
            ViewData["CurrentUserId"] = currentUserId;

            // Проверяем, оценил ли текущий пользователь пост
            if (currentUserId.HasValue)
            {
                var existingPostRating = await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == currentUserId.Value && r.PostId == post.Id);
                ViewData["ExistingPostRating"] = existingPostRating;
            }

            // Получаем рейтинги для каждого комментария и вычисляем их сумму и количество
            var commentTotalRatings = new Dictionary<int, int>();
            var commentRatingsCounts = new Dictionary<int, int>();
            if (comments != null) // Добавлена проверка на null
            {
                foreach (var comment in comments)
                {
                    var currentCommentRatings = await _context.Ratings
                                                            .Where(r => r.CommentId == comment.Id)
                                                            .ToListAsync();
                    commentTotalRatings[comment.Id] = currentCommentRatings.Sum(r => r.Value ? 1 : -1);
                    commentRatingsCounts[comment.Id] = currentCommentRatings.Count;
                }
            }
            ViewData["CommentTotalRatings"] = commentTotalRatings;
            ViewData["CommentRatingsCounts"] = commentRatingsCounts;

            // Pre-fetch existing ratings for the current user for all comments
            if (currentUserId.HasValue)
            {
                var userCommentRatings = await _context.Ratings
                                                       .Where(r => r.UserId == currentUserId.Value && r.CommentId.HasValue && comments.Select(c => c.Id).Contains(r.CommentId.Value))
                                                       .ToDictionaryAsync(r => r.CommentId.Value, r => r);
                ViewData["ExistingCommentRatings"] = userCommentRatings;
            }
            else
            {
                ViewData["ExistingCommentRatings"] = new Dictionary<int, Rating>(); // Пустой словарь, если пользователь не аутентифицирован
            }

            return View(post);
        }

        // GET: Posts/Create
        // Отображает форму для создания нового поста.
        public IActionResult Create()
        {
            // Здесь не нужно передавать AuthorId, так как он будет установлен на сервере
            return View();
        }

        // POST: Posts/Create
        // Обрабатывает отправку формы для создания нового поста.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Исключаем AuthorId из привязки, чтобы он не приходил из формы
        public async Task<IActionResult> Create([Bind("Title,Content,IsHidden")] Post post)
        {
            // Получаем ID текущего залогиненного пользователя
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            if (!currentUserId.HasValue)
            {
                // Если пользователь не залогинен, можно перенаправить на страницу логина
                // или добавить ошибку в ModelState
                ModelState.AddModelError(string.Empty, "Вы должны быть залогинены, чтобы создать пост.");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                post.AuthorId = currentUserId.Value; // Устанавливаем AuthorId из залогиненного пользователя
                post.CreationDate = DateTime.UtcNow; // Устанавливаем дату создания
                post.LastEditDate = null; // Дата последнего редактирования отсутствует, так как пост новый
                post.Rating = 0; // Начальный рейтинг

                _context.Add(post); // Добавляем пост в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список постов
            }
            // Если модель невалидна, возвращаем форму с ошибками
            return View(post);
        }

        // GET: Posts/Edit/5
        // Отображает форму для редактирования существующего поста.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id); // Находим пост по Id
            if (post == null)
            {
                return NotFound();
            }
            // Проверка на то, является ли текущий пользователь автором поста
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            if (!currentUserId.HasValue || post.AuthorId != currentUserId.Value)
            {
                return Forbid(); // Или NotFound(), в зависимости от вашей логики доступа
            }

            return View(post);
        }

        // POST: Posts/Edit/5
        // Обрабатывает отправку формы для редактирования поста.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Убедитесь, что все поля, которые могут быть изменены пользователем, включены.
        // CreationDate, Id, AuthorId, Rating не должны меняться через форму редактирования.
        // LastEditDate устанавливается здесь.
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,IsHidden")] Post post)
        {
            if (id != post.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            // Получаем ID текущего залогиненного пользователя
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            // Загружаем существующий пост из базы данных для проверки AuthorId
            var existingPost = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existingPost == null)
            {
                return NotFound();
            }

            // Проверяем, что текущий пользователь является автором поста
            if (!currentUserId.HasValue || existingPost.AuthorId != currentUserId.Value)
            {
                return Forbid(); // Пользователь не имеет прав на редактирование этого поста
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.LastEditDate = DateTime.UtcNow; // Обновляем дату последнего редактирования
                    post.CreationDate = existingPost.CreationDate; // Сохраняем оригинальную дату создания
                    post.AuthorId = existingPost.AuthorId; // Сохраняем оригинального автора
                    post.Rating = existingPost.Rating; // Сохраняем текущий рейтинг

                    _context.Update(post); // Обновляем пост в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!PostExists(post.Id))
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
            return View(post);
        }

        // GET: Posts/Delete/5
        // Отображает страницу подтверждения удаления поста.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            // Проверка на то, является ли текущий пользователь автором поста
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            if (!currentUserId.HasValue || post.AuthorId != currentUserId.Value)
            {
                return Forbid(); // Или NotFound(), в зависимости от вашей логики доступа
            }

            // Получаем имя автора для отображения на странице удаления
            var author = await _context.Users.FindAsync(post.AuthorId);
            ViewData["AuthorName"] = author?.Login ?? "Неизвестный автор";

            return View(post);
        }

        // POST: Posts/Delete/5
        // Обрабатывает подтверждение удаления поста.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Проверка на то, является ли текущий пользователь автором поста
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            if (!currentUserId.HasValue || post.AuthorId != currentUserId.Value)
            {
                return Forbid(); // Пользователь не имеет прав на удаление этого поста
            }

            _context.Posts.Remove(post); // Удаляем пост из контекста
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования поста
        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}