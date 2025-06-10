using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // Для Task
using System; // Для DateTime
using System.Linq; // Для Any()
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList
using System.Collections.Generic; // For Dictionary

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
            var complaints = await _context.Complaints.ToListAsync();

            // Collect unique IDs for related entities
            var userIds = complaints.Select(c => c.IdUser).Distinct().ToList();
            var adminIds = complaints.Where(c => c.IdAdministrator.HasValue).Select(c => c.IdAdministrator.Value).Distinct().ToList();
            var postIds = complaints.Where(c => c.IdPost.HasValue).Select(c => c.IdPost.Value).Distinct().ToList();
            var commentIds = complaints.Where(c => c.IdComment.HasValue).Select(c => c.IdComment.Value).Distinct().ToList();
            var eventIds = complaints.Where(c => c.IdEvent.HasValue).Select(c => c.IdEvent.Value).Distinct().ToList();
            var messageIds = complaints.Where(c => c.IdMessage.HasValue).Select(c => c.IdMessage.Value).Distinct().ToList();

            // Fetch names/logins
            var userLogins = await _context.Users.Where(u => userIds.Contains(u.IdUser)).ToDictionaryAsync(u => u.IdUser, u => u.Login);
            // Assuming administrator has a link to User, or a simple display name
            // For now, just show the IdAdministrator
            var postTitles = await _context.Posts.Where(p => postIds.Contains(p.IdPost)).ToDictionaryAsync(p => p.IdPost, p => p.Title);
            var commentContents = await _context.Comments.Where(c => commentIds.Contains(c.IdComment)).ToDictionaryAsync(c => c.IdComment, c => c.Content);
            var eventNames = await _context.Events.Where(e => eventIds.Contains(e.IdEvent)).ToDictionaryAsync(e => e.IdEvent, e => e.Name);
            var messageContents = await _context.Messages.Where(m => messageIds.Contains(m.IdMessage)).ToDictionaryAsync(m => m.IdMessage, m => m.Content);


            ViewData["UserLogins"] = userLogins;
            ViewData["PostTitles"] = postTitles;
            ViewData["CommentContents"] = commentContents;
            ViewData["EventNames"] = eventNames;
            ViewData["MessageContents"] = messageContents;

            return View(complaints);
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
                .FirstOrDefaultAsync(m => m.IdComplaint == id); // CORRECTED: Use IdComplaint
            if (complaint == null)
            {
                return NotFound(); // Если жалоба не найдена, возвращаем 404
            }

            // Fetch related details for display
            ViewData["UserLogin"] = (await _context.Users.FirstOrDefaultAsync(u => u.IdUser == complaint.IdUser))?.Login ?? "N/A";
            ViewData["AdministratorLogin"] = complaint.IdAdministrator.HasValue ?
                (await _context.Administrators.Where(a => a.IdAdministrator == complaint.IdAdministrator.Value)
                                             .Join(_context.Users, a => a.IdUser, u => u.IdUser, (a, u) => u.Login)
                                             .FirstOrDefaultAsync()) ?? "Не назначен"
                : "Не назначен";
            ViewData["PostTitle"] = complaint.IdPost.HasValue ?
                (await _context.Posts.FirstOrDefaultAsync(p => p.IdPost == complaint.IdPost.Value))?.Title ?? "N/A"
                : "Не применимо";
            ViewData["CommentContent"] = complaint.IdComment.HasValue ?
                (await _context.Comments.FirstOrDefaultAsync(c => c.IdComment == complaint.IdComment.Value))?.Content ?? "N/A"
                : "Не применимо";
            ViewData["EventName"] = complaint.IdEvent.HasValue ?
                (await _context.Events.FirstOrDefaultAsync(e => e.IdEvent == complaint.IdEvent.Value))?.Name ?? "N/A"
                : "Не применимо";
            ViewData["MessageContent"] = complaint.IdMessage.HasValue ?
                (await _context.Messages.FirstOrDefaultAsync(m => m.IdMessage == complaint.IdMessage.Value))?.Content ?? "N/A"
                : "Не применимо";


            return View(complaint);
        }

        // GET: Complaints/Create
        // Отображает форму для создания новой жалобы.
        public async Task<IActionResult> Create()
        {
            // For selecting related entities, populate SelectLists
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login");
            ViewBag.Posts = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title");
            ViewBag.Comments = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content");
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name");
            ViewBag.Messages = new SelectList(await _context.Messages.ToListAsync(), "IdMessage", "Content"); // Assuming message content is sufficient for selection
            ViewBag.Administrators = new SelectList( // Only administrators who are users
                await _context.Administrators
                              .Join(_context.Users, a => a.IdUser, u => u.IdUser, (a, u) => new { a.IdAdministrator, u.Login })
                              .ToListAsync(),
                "IdAdministrator", "Login");

            return View();
        }

        // POST: Complaints/Create
        // Обрабатывает отправку формы для создания новой жалобы.
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        // CORRECTED: Bind to model properties (IdUser, IdPost, IdComment, IdEvent, IdMessage, IdAdministrator)
        public async Task<IActionResult> Create([Bind("IdUser,IdPost,IdComment,IdEvent,IdMessage,Reason,IdAdministrator,ModeratorNotes,Status")] Complaint complaint)
        {
            // Ensure only one of PostId, CommentId, EventId, MessageId is provided.
            int relatedEntityCount = 0;
            if (complaint.IdPost.HasValue) relatedEntityCount++;
            if (complaint.IdComment.HasValue) relatedEntityCount++;
            if (complaint.IdEvent.HasValue) relatedEntityCount++;
            if (complaint.IdMessage.HasValue) relatedEntityCount++;

            if (relatedEntityCount == 0)
            {
                ModelState.AddModelError("", "Жалоба должна быть связана хотя бы с одним объектом (пост, комментарий, мероприятие, сообщение).");
            }
            else if (relatedEntityCount > 1)
            {
                ModelState.AddModelError("", "Жалоба может быть связана только с одним объектом (пост, комментарий, мероприятие, сообщение).");
            }


            if (ModelState.IsValid) // Проверяем валидность модели на основе Data Annotations и ручных проверок
            {
                complaint.CreationDate = DateTime.UtcNow; // CORRECTED: Use CreationDate
                // Status по умолчанию уже установлен в модели как Pending.
                // ResolutionDate будет null по умолчанию.

                _context.Add(complaint); // Добавляем жалобу в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                TempData["SuccessMessage"] = "Жалоба успешно создана.";
                return RedirectToAction(nameof(Index)); // Перенаправляем на список жалоб
            }

            // Если модель невалидна, возвращаем форму с ошибками
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", complaint.IdUser);
            ViewBag.Posts = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", complaint.IdPost);
            ViewBag.Comments = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content", complaint.IdComment);
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name", complaint.IdEvent);
            ViewBag.Messages = new SelectList(await _context.Messages.ToListAsync(), "IdMessage", "Content", complaint.IdMessage);
            ViewBag.Administrators = new SelectList(
                await _context.Administrators
                              .Join(_context.Users, a => a.IdUser, u => u.IdUser, (a, u) => new { a.IdAdministrator, u.Login })
                              .ToListAsync(),
                "IdAdministrator", "Login", complaint.IdAdministrator);
            TempData["ErrorMessage"] = "Ошибка при создании жалобы. Проверьте введенные данные.";
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

            var complaint = await _context.Complaints.FindAsync(id); // CORRECTED: Id is implicitly IdComplaint if it's the primary key
            if (complaint == null)
            {
                return NotFound();
            }

            // For editing related Id's or status, if they should be editable.
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", complaint.IdUser);
            ViewBag.Posts = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", complaint.IdPost);
            ViewBag.Comments = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content", complaint.IdComment);
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name", complaint.IdEvent);
            ViewBag.Messages = new SelectList(await _context.Messages.ToListAsync(), "IdMessage", "Content", complaint.IdMessage);
            ViewBag.Administrators = new SelectList(
                await _context.Administrators
                              .Join(_context.Users, a => a.IdUser, u => u.IdUser, (a, u) => new { a.IdAdministrator, u.Login })
                              .ToListAsync(),
                "IdAdministrator", "Login", complaint.IdAdministrator);

            return View(complaint);
        }

        // POST: Complaints/Edit/5
        // Обрабатывает отправку формы для редактирования жалобы.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CORRECTED: Bind all potentially editable fields, including the foreign keys if they can change.
        // IdUser, IdPost, IdComment, IdEvent, IdMessage should ideally not be changed after creation,
        // but if they are, they need to be in the bind and re-validated.
        // For now, assuming they are not changed via simple binding, and loaded from existingComplaint.
        public async Task<IActionResult> Edit(int id, [Bind("IdComplaint,Reason,Status,IdAdministrator,ModeratorNotes")] Complaint complaint) // CORRECTED: IdComplaint in Bind
        {
            if (id != complaint.IdComplaint) // CORRECTED: Check against IdComplaint
            {
                return NotFound();
            }

            // Load existing entry to preserve immutable fields like IdUser, CreationDate, and the related entity IDs
            var existingComplaint = await _context.Complaints.AsNoTracking().FirstOrDefaultAsync(c => c.IdComplaint == id); // CORRECTED: IdComplaint
            if (existingComplaint == null)
            {
                return NotFound();
            }

            // Transfer immutable fields from the existing record to the bound model
            complaint.IdUser = existingComplaint.IdUser; // CORRECTED: Use IdUser
            complaint.CreationDate = existingComplaint.CreationDate; // CORRECTED: Use CreationDate
            complaint.IdPost = existingComplaint.IdPost; // CORRECTED: Use IdPost
            complaint.IdComment = existingComplaint.IdComment; // CORRECTED: Use IdComment
            complaint.IdEvent = existingComplaint.IdEvent; // ADDED: Transfer IdEvent
            complaint.IdMessage = existingComplaint.IdMessage; // ADDED: Transfer IdMessage


            // Additional validation logic: A complaint must be related to exactly one object.
            int relatedEntityCount = 0;
            if (complaint.IdPost.HasValue) relatedEntityCount++;
            if (complaint.IdComment.HasValue) relatedEntityCount++;
            if (complaint.IdEvent.HasValue) relatedEntityCount++;
            if (complaint.IdMessage.HasValue) relatedEntityCount++;

            if (relatedEntityCount == 0)
            {
                ModelState.AddModelError("", "Жалоба должна быть связана хотя бы с одним объектом (пост, комментарий, мероприятие, сообщение).");
            }
            else if (relatedEntityCount > 1)
            {
                ModelState.AddModelError("", "Жалоба может быть связана только с одним объектом (пост, комментарий, мероприятие, сообщение).");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // Logic to set ResolutionDate based on status
                    if ((complaint.Status == ComplaintStatus.Resolved || complaint.Status == ComplaintStatus.Rejected) && !complaint.ResolutionDate.HasValue) // CORRECTED: Use ResolutionDate
                    {
                        complaint.ResolutionDate = DateTime.UtcNow; // CORRECTED: Use ResolutionDate
                    }
                    else if (complaint.Status != ComplaintStatus.Resolved && complaint.Status != ComplaintStatus.Rejected)
                    {
                        complaint.ResolutionDate = null; // CORRECTED: Use ResolutionDate. Reset if status is not Resolved/Rejected
                    }

                    _context.Update(complaint); // Update the complaint in the context
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Жалоба успешно обновлена.";
                }
                catch (DbUpdateConcurrencyException) // Handle concurrency conflicts
                {
                    if (!ComplaintExists(complaint.IdComplaint)) // CORRECTED: Check against IdComplaint
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throw the exception if it's not a concurrency conflict
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // If the model is invalid, return the form with errors
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", complaint.IdUser);
            ViewBag.Posts = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", complaint.IdPost);
            ViewBag.Comments = new SelectList(await _context.Comments.ToListAsync(), "IdComment", "Content", complaint.IdComment);
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "IdEvent", "Name", complaint.IdEvent);
            ViewBag.Messages = new SelectList(await _context.Messages.ToListAsync(), "IdMessage", "Content", complaint.IdMessage);
            ViewBag.Administrators = new SelectList(
                await _context.Administrators
                              .Join(_context.Users, a => a.IdUser, u => u.IdUser, (a, u) => new { a.IdAdministrator, u.Login })
                              .ToListAsync(),
                "IdAdministrator", "Login", complaint.IdAdministrator);
            TempData["ErrorMessage"] = "Ошибка при обновлении жалобы. Проверьте введенные данные.";
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
                .FirstOrDefaultAsync(m => m.IdComplaint == id); // CORRECTED: Use IdComplaint
            if (complaint == null)
            {
                return NotFound();
            }

            // Fetch related details for display on delete confirmation page
            ViewData["UserLogin"] = (await _context.Users.FirstOrDefaultAsync(u => u.IdUser == complaint.IdUser))?.Login ?? "N/A";
            ViewData["AdministratorLogin"] = complaint.IdAdministrator.HasValue ?
                (await _context.Administrators.Where(a => a.IdAdministrator == complaint.IdAdministrator.Value)
                                             .Join(_context.Users, a => a.IdUser, u => u.IdUser, (a, u) => u.Login)
                                             .FirstOrDefaultAsync()) ?? "Не назначен"
                : "Не назначен";
            ViewData["PostTitle"] = complaint.IdPost.HasValue ?
                (await _context.Posts.FirstOrDefaultAsync(p => p.IdPost == complaint.IdPost.Value))?.Title ?? "N/A"
                : "Не применимо";
            ViewData["CommentContent"] = complaint.IdComment.HasValue ?
                (await _context.Comments.FirstOrDefaultAsync(c => c.IdComment == complaint.IdComment.Value))?.Content ?? "N/A"
                : "Не применимо";
            ViewData["EventName"] = complaint.IdEvent.HasValue ?
                (await _context.Events.FirstOrDefaultAsync(e => e.IdEvent == complaint.IdEvent.Value))?.Name ?? "N/A"
                : "Не применимо";
            ViewData["MessageContent"] = complaint.IdMessage.HasValue ?
                (await _context.Messages.FirstOrDefaultAsync(m => m.IdMessage == complaint.IdMessage.Value))?.Content ?? "N/A"
                : "Не применимо";

            return View(complaint);
        }

        // POST: Complaints/Delete/5
        // Обрабатывает подтверждение удаления жалобы.
        [HttpPost, ActionName("Delete")] // Указываем, что это POST-действие для маршрута Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id); // CORRECTED: Id is implicitly IdComplaint if it's the primary key
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint); // Удаляем жалобу из контекста
            }

            await _context.SaveChangesAsync(); // Сохраняем изменения
            TempData["SuccessMessage"] = "Жалоба успешно удалена.";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки существования жалобы
        private bool ComplaintExists(int id)
        {
            return _context.Complaints.Any(e => e.IdComplaint == id); // CORRECTED: Use IdComplaint
        }
    }
}