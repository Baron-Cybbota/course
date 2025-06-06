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
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Comments
        // Отображает список всех комментариев.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Comments.ToListAsync());
        }

        // GET: Comments/Details/5
        // Отображает детали конкретного комментария по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound(); // Если комментарий не найден, возвращаем 404
            }

            return View(comment);
        }

        // GET: Comments/Create
        // Отображает форму для создания нового комментария.
        public async Task<IActionResult> Create()
        {
            // Для выбора PostId и AuthorId, если это требуется на форме.
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title");
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login");
            return View();
        }

        // POST: Comments/Create
        // Обрабатывает отправку формы для создания нового комментария.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
         // Поля, которые пользователь может ввести. CreationDate, Rating устанавливаются сервером.
        public async Task<IActionResult> Create([Bind("PostId,AuthorId,Content")] Comment comment)
        {
            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations
            {
                comment.CreationDate = DateTime.UtcNow; // Устанавливаем дату создания
                comment.Rating = 0; // Начальный рейтинг
                
                _context.Add(comment); // Добавляем комментарий в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список комментариев
            }
            // Если модель невалидна, возвращаем форму с ошибками
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", comment.PostId);
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", comment.AuthorId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        // Отображает форму для редактирования существующего комментария.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id); // Находим комментарий по Id
            if (comment == null)
            {
                return NotFound();
            }
            // Для редактирования PostId или AuthorId, если они должны быть редактируемыми.
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", comment.PostId);
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", comment.AuthorId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // Обрабатывает отправку формы для редактирования комментария.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind("Id,PostId,AuthorId,Content,CreationDate,Rating")
        // Убедитесь, что все поля, которые могут быть изменены пользователем, включены.
        // CreationDate, Id, PostId, AuthorId, Rating обычно не меняются через форму редактирования.
        public async Task<IActionResult> Edit(int id, [Bind("Id,PostId,AuthorId,Content,CreationDate,Rating")] Comment comment)
        {
            if (id != comment.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Сохраняем текущие значения, которые не должны меняться через форму
                    var existingComment = await _context.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingComment == null)
                    {
                        return NotFound();
                    }
                    
                    // Обновляем только поля, которые могут быть изменены пользователем
                    // Content уже в Bind, но если были бы другие поля для редактирования
                    comment.CreationDate = existingComment.CreationDate; // Сохраняем оригинальную дату создания
                    comment.PostId = existingComment.PostId; // Сохраняем оригинальный PostId
                    comment.AuthorId = existingComment.AuthorId; // Сохраняем оригинального автора
                    comment.Rating = existingComment.Rating; // Сохраняем текущий рейтинг

                    _context.Update(comment); // Обновляем комментарий в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!CommentExists(comment.Id))
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
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", comment.PostId);
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", comment.AuthorId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        // Отображает страницу подтверждения удаления комментария.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        // Обрабатывает подтверждение удаления комментария.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment); // Удаляем комментарий из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования комментария
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
