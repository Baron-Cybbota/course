using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq; // Для Linq методов (Any)

namespace course.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("PostId,Content")] Comment comment)
        {
            // Проверяем, существует ли пост, к которому пытаются добавить комментарий.
            // Хотя PostId является внешним ключом, явная проверка добавляет надежности.
            var postExists = await _context.Posts.AnyAsync(p => p.Id == comment.PostId && !p.IsHidden);
            if (!postExists)
            {
                // Если пост не существует или скрыт, возвращаем NotFound или ошибку.
                return NotFound("Пост, к которому вы пытаетесь добавить комментарий, не найден или скрыт.");
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "Вы должны быть авторизованы, чтобы оставить комментарий.");
                    // Возможно, лучше перенаправить на страницу логина или на страницу поста с сообщением.
                    return RedirectToAction("Details", "Posts", new { id = comment.PostId });
                }

                comment.AuthorId = int.Parse(userId);
                comment.CreatedDate = DateTime.UtcNow;
                comment.IsEdited = false; // Новый комментарий, не редактировался

                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Posts", new { id = comment.PostId }); // Вернуться на страницу поста
            }
            // Если ModelState невалиден (например, Content пустой),
            // возвращаем пользователя обратно на страницу поста с ошибкой.
            // В идеале, ошибки должны быть переданы в представление.
            // Для простоты сейчас просто перенаправляем, но это может привести к потере сообщений об ошибках валидации.
            // Рекомендуется использовать ViewModel для передачи ошибок обратно на форму.
            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }
		
        // GET: Comments/Edit/5
		[Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Проверка: только автор комментария или модератор могут редактировать
            if (comment.AuthorId != int.Parse(userId) && !User.IsInRole("Модератор"))
            {
                return Forbid(); // Запрещаем доступ
            }

            return View(comment);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        // Убираем CommentDate и IsDeleted из Bind, так как они управляются сервером
        public async Task<IActionResult> Edit(int id, [Bind("Id,PostId,Content")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            // Получаем существующий комментарий из базы данных для сохранения неизменяемых полей
            var existingComment = await _context.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingComment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Повторная проверка прав доступа
            if (existingComment.AuthorId != int.Parse(userId) && !User.IsInRole("Модератор"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Сохраняем Original AuthorId и CreatedDate
                    comment.AuthorId = existingComment.AuthorId;
                    comment.CreatedDate = existingComment.CreatedDate;
                    comment.IsEdited = true; // Отмечаем, что комментарий был отредактирован
                    comment.LastEditedDate = DateTime.UtcNow; // Обновляем дату последнего редактирования

                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Повторно бросаем исключение, если это не ошибка "не найден"
                    }
                }
                return RedirectToAction("Details", "Posts", new { id = comment.PostId });
            }
            return View(comment); // Возвращаем представление с ошибками валидации, если они есть
        }

        // GET: Comments/Delete/5
        [Authorize(Roles = "Модератор")] // Только модераторы могут видеть страницу удаления
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            // Загрузка имени автора комментария для отображения в представлении
            var author = await _userManager.FindByIdAsync(comment.AuthorId.ToString());
            ViewData["AuthorName"] = author?.UserName;

            // Загрузка заголовка поста для отображения в представлении
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == comment.PostId);
            ViewData["PostTitle"] = post?.Title;

            // Если разрешено автору удалять свой комментарий, добавьте проверку здесь
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (comment.AuthorId != int.Parse(userId) && !User.IsInRole("Модератор"))
            // {
            //     return Forbid();
            // }

            return View(comment);
        }

        // POST: Comments/Delete/5 (DeleteConfirmed)
        [HttpPost, ActionName("Delete")] // Связываем с методом Delete (GET)
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Модератор")] // Только модераторы могут подтвердить удаление
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(); // Комментарий уже удален или не существует
            }

            // Сохраняем PostId, чтобы знать, куда перенаправить после удаления
            var postId = comment.PostId;

            // Если есть поле IsDeleted в модели Comment, используйте логическое удаление:
            // comment.IsDeleted = true;
            // _context.Update(comment);
            // Вместо физического удаления:
            _context.Comments.Remove(comment); // Физическое удаление комментария из базы данных
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = postId }); // Перенаправить на страницу поста
        }

        // Вспомогательный метод CommentExists
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}