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
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Complaints
        // Отображает список всех жалоб.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Complaints.ToListAsync());
        }

        // GET: Complaints/Details/5
        // Отображает детали конкретной жалобы по Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Если Id не предоставлен, возвращаем 404
            }

            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(m => m.Id == id);
            if (complaint == null)
            {
                return NotFound(); // Если жалоба не найдена, возвращаем 404
            }

            return View(complaint);
        }

        // GET: Complaints/Create
        // Отображает форму для создания новой жалобы.
        public async Task<IActionResult> Create()
        {
            // Для выбора AuthorId, PostId и CommentId, если это требуется на форме.
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login");
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title");
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content");
            return View();
        }

        // POST: Complaints/Create
        // Обрабатывает отправку формы для создания новой жалобы.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // Поля, которые пользователь может ввести. CreatedDate и ResolvedDate не включаем,
        // так как они устанавливаются сервером или логикой.
        
        public async Task<IActionResult> Create([Bind("AuthorId,PostId,CommentId,Reason,Status,AdministratorId,ModeratorNotes")] Complaint complaint)
        {
            // Дополнительная логика проверки: жалоба должна быть либо на пост, либо на комментарий, но не на оба.
            if (complaint.PostId.HasValue && complaint.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Жалоба должна быть либо на пост, либо на комментарий, но не на оба.");
            }
            if (!complaint.PostId.HasValue && !complaint.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Жалоба должна быть либо на пост, либо на комментарий.");
            }

            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations и ручных проверок
            {
                complaint.CreatedDate = DateTime.UtcNow; // Устанавливаем дату создания
                // Status по умолчанию уже установлен в модели как Pending.
                // ResolvedDate будет null по умолчанию.
                
                _context.Add(complaint); // Добавляем жалобу в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // Перенаправляем на список жалоб
            }
            // Если модель невалидна, возвращаем форму с ошибками
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", complaint.AuthorId);
            // ViewBag.AdministratorId = new SelectList(await _context.Administrators.ToListAsync(), "Id", "AccessLevel", complaint.AdministratorId);
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", complaint.PostId);
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content", complaint.CommentId);
            return View(complaint);
        }

        // GET: Complaints/Edit/5
        // Отображает форму для редактирования существующей жалобы.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complaint = await _context.Complaints.FindAsync(id); // Находим жалобу по Id
            if (complaint == null)
            {
                return NotFound();
            }
            // Для редактирования связанных Id или статуса, если они должны быть редактируемыми.
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", complaint.AuthorId);
            // ViewBag.AdministratorId = new SelectList(await _context.Administrators.ToListAsync(), "Id", "AccessLevel", complaint.AdministratorId);
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", complaint.PostId);
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content", complaint.CommentId);
            return View(complaint);
        }

        // POST: Complaints/Edit/5
        // Обрабатывает отправку формы для редактирования жалобы.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Убедитесь, что все поля, которые могут быть изменены пользователем, включены.
        // Id, CreatedDate, AuthorId, PostId, CommentId обычно не меняются через форму редактирования после создания.
        // AdministratorId, ResolvedDate, ModeratorNotes, Status могут изменяться.
        public async Task<IActionResult> Edit(int id, [Bind("Id,Reason,Status,AdministratorId,ResolvedDate,ModeratorNotes")] Complaint complaint)
        {
            if (id != complaint.Id) // Проверяем, совпадает ли Id из маршрута с Id модели
            {
                return NotFound();
            }

            // Загружаем существующую запись, чтобы сохранить неизменяемые поля.
            // Используем AsNoTracking, чтобы не отслеживать сущность, которую мы потом обновим.
            var existingComplaint = await _context.Complaints.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingComplaint == null)
            {
                return NotFound();
            }

            // Переносим неизменяемые поля из существующей записи
            complaint.AuthorId = existingComplaint.AuthorId;
            complaint.CreatedDate = existingComplaint.CreatedDate;
            complaint.PostId = existingComplaint.PostId;
            complaint.CommentId = existingComplaint.CommentId;

            // Дополнительная логика проверки: жалоба должна быть либо на пост, либо на комментарий, но не на оба.
            // Эти проверки должны быть проведены после переноса PostId/CommentId
            if (complaint.PostId.HasValue && complaint.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Жалоба должна быть либо на пост, либо на комментарий, но не на оба.");
            }
            if (!complaint.PostId.HasValue && !complaint.CommentId.HasValue)
            {
                ModelState.AddModelError("", "Жалоба должна быть либо на пост, либо на комментарий.");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // Логика установки ResolvedDate в зависимости от статуса
                    if ((complaint.Status == ComplaintStatus.Resolved || complaint.Status == ComplaintStatus.Rejected) && !complaint.ResolvedDate.HasValue)
                    {
                        complaint.ResolvedDate = DateTime.UtcNow;
                    }
                    else if (complaint.Status != ComplaintStatus.Resolved && complaint.Status != ComplaintStatus.Rejected)
                    {
                        complaint.ResolvedDate = null; // Сбрасываем дату решения, если статус не "Решена" или "Отклонена"
                    }

                    _context.Update(complaint); // Обновляем жалобу в контексте
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Обработка конфликтов параллельного доступа
                {
                    if (!ComplaintExists(complaint.Id))
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
            // ViewBag.AuthorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", complaint.AuthorId);
            // ViewBag.AdministratorId = new SelectList(await _context.Administrators.ToListAsync(), "Id", "AccessLevel", complaint.AdministratorId);
            // ViewBag.PostId = new SelectList(await _context.Posts.ToListAsync(), "Id", "Title", complaint.PostId);
            // ViewBag.CommentId = new SelectList(await _context.Comments.ToListAsync(), "Id", "Content", complaint.CommentId);
            return View(complaint);
        }

        // GET: Complaints/Delete/5
        // Отображает страницу подтверждения удаления жалобы.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(m => m.Id == id);
            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        // POST: Complaints/Delete/5
        // Обрабатывает подтверждение удаления жалобы.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint); // Удаляем жалобу из контекста
            }
            
            await _context.SaveChangesAsync(); // Сохраняем изменения
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования жалобы
        private bool ComplaintExists(int id)
        {
            return _context.Complaints.Any(e => e.Id == id);
        }
    }
}
