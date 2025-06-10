using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using course.Models.ViewModels; // Still assuming you'll use these ViewModels
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

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
        /// <summary>
        /// Displays a list of all comments.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments.ToListAsync();

            // Explicitly fetch Post Titles and User Logins for display
            var postTitles = new Dictionary<int, string>();
            var userLogins = new Dictionary<int, string>();

            // Get all unique Post Ids and User Ids from comments
            var postIds = comments.Select(c => c.IdPost).Distinct().ToList();
            var userIds = comments.Select(c => c.IdUser).Distinct().ToList();

            // Fetch all necessary post titles and user logins in batches
            var posts = await _context.Posts
                                      .Where(p => postIds.Contains(p.IdPost))
                                      .Select(p => new { p.IdPost, p.Title })
                                      .ToListAsync();

            var users = await _context.Users
                                    .Where(u => userIds.Contains(u.IdUser))
                                    .Select(u => new { u.IdUser, u.Login })
                                    .ToListAsync();

            foreach (var post in posts)
            {
                postTitles[post.IdPost] = post.Title;
            }

            foreach (var user in users)
            {
                userLogins[user.IdUser] = user.Login;
            }

            ViewData["PostTitles"] = postTitles;
            ViewData["UserLogins"] = userLogins;

            return View(comments);
        }

        // GET: Comments/Details/5
        /// <summary>
        /// Displays details of a specific comment by Id.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find comment using IdComment
            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.IdComment == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Explicitly fetch related Post Title and User Login
            var postTitle = await _context.Posts
                                            .Where(p => p.IdPost == comment.IdPost)
                                            .Select(p => p.Title)
                                            .FirstOrDefaultAsync();
            var userLogin = await _context.Users
                                            .Where(u => u.IdUser == comment.IdUser)
                                            .Select(u => u.Login)
                                            .FirstOrDefaultAsync();

            ViewData["PostTitle"] = postTitle ?? "Неизвестная публикация";
            ViewData["UserLogin"] = userLogin ?? "Неизвестный пользователь";

            return View(comment);
        }

        // GET: Comments/Create
        /// <summary>
        /// Displays the form for creating a new comment.
        /// </summary>
        public async Task<IActionResult> Create()
        {
            // Populate SelectLists for the ViewModel
            var posts = await _context.Posts.OrderBy(p => p.Title).ToListAsync();
            var users = await _context.Users.OrderBy(u => u.Login).ToListAsync();

            var viewModel = new CommentCreateViewModel
            {
                PostsSelectList = new SelectList(posts, "IdPost", "Title"),
                UsersSelectList = new SelectList(users, "IdUser", "Login")
            };

            return View(viewModel); // Pass the ViewModel to the view
        }

        // POST: Comments/Create
        /// <summary>
        /// Handles the form submission for creating a new comment.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPost,IdUser,Content")] CommentCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var comment = new Comment
                {
                    IdPost = viewModel.IdPost,
                    IdUser = viewModel.IdUser,
                    Content = viewModel.Content,
                    CreationDate = DateTime.UtcNow,
                    Rating = 0 // Initial rating
                };

                _context.Add(comment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Комментарий успешно добавлен.";
                return RedirectToAction(nameof(Index));
            }

            // If model is invalid, re-populate SelectLists for the ViewModel and return the view
            var posts = await _context.Posts.OrderBy(p => p.Title).ToListAsync();
            var users = await _context.Users.OrderBy(u => u.Login).ToListAsync();
            viewModel.PostsSelectList = new SelectList(posts, "IdPost", "Title", viewModel.IdPost);
            viewModel.UsersSelectList = new SelectList(users, "IdUser", "Login", viewModel.IdUser);

            TempData["ErrorMessage"] = "Ошибка при создании комментария. Проверьте введенные данные.";
            return View(viewModel); // Pass the ViewModel back with errors
        }

        // GET: Comments/Edit/5
        /// <summary>
        /// Displays the form for editing an existing comment.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id); // Find by IdComment
            if (comment == null)
            {
                return NotFound();
            }
            // Populate SelectLists for IdPost and IdUser (even if not directly editable by user)
            ViewBag.IdPost = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", comment.IdPost);
            ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", comment.IdUser);

            return View(comment);
        }

        // POST: Comments/Edit/5
        /// <summary>
        /// Handles the form submission for editing a comment.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdComment,Content")] Comment comment)
        {
            if (id != comment.IdComment)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Load the existing entry to preserve immutable fields
                    var existingComment = await _context.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.IdComment == id);
                    if (existingComment == null)
                    {
                        return NotFound();
                    }

                    // Transfer immutable fields from the existing entity
                    comment.CreationDate = existingComment.CreationDate;
                    comment.IdPost = existingComment.IdPost; // Preserve original Post
                    comment.IdUser = existingComment.IdUser; // Preserve original Author
                    comment.Rating = existingComment.Rating; // Preserve original Rating
                    comment.EditDate = DateTime.UtcNow; // Set EditDate when modified

                    _context.Update(comment); // Mark the entity for update
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Комментарий успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.IdComment))
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
            // If model is invalid, return the form with errors, re-populating ViewBag
            ViewBag.IdPost = new SelectList(await _context.Posts.ToListAsync(), "IdPost", "Title", comment.IdPost);
            ViewBag.IdUser = new SelectList(await _context.Users.ToListAsync(), "IdUser", "Login", comment.IdUser);
            TempData["ErrorMessage"] = "Ошибка при обновлении комментария. Проверьте введенные данные.";
            return View(comment);
        }

        // GET: Comments/Delete/5
        /// <summary>
        /// Displays the confirmation page for deleting a comment.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.IdComment == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Explicitly fetch related Post Title and User Login for display
            var postTitle = await _context.Posts
                                            .Where(p => p.IdPost == comment.IdPost)
                                            .Select(p => p.Title)
                                            .FirstOrDefaultAsync();
            var userLogin = await _context.Users
                                            .Where(u => u.IdUser == comment.IdUser)
                                            .Select(u => u.Login)
                                            .FirstOrDefaultAsync();

            // Populate the ViewModel for the delete confirmation view
            var viewModel = new CommentDeleteViewModel
            {
                IdComment = comment.IdComment,
                IdPost = comment.IdPost, // Crucial for redirection
                Content = comment.Content,
                CreationDate = comment.CreationDate,
                PostTitle = postTitle ?? "Неизвестная публикация",
                AuthorName = userLogin ?? "Неизвестный пользователь"
            };

            return View(viewModel); // Pass the ViewModel to the view
        }

        // POST: Comments/Delete/5
        /// <summary>
        /// Handles the confirmation of deleting a comment.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commentToDelete = await _context.Comments.FindAsync(id);

            if (commentToDelete == null)
            {
                TempData["ErrorMessage"] = "Комментарий не найден для удаления.";
                return NotFound();
            }

            // Capture IdPost BEFORE deleting for redirection if needed
            var postIdForRedirection = commentToDelete.IdPost;

            _context.Comments.Remove(commentToDelete);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Комментарий успешно удален.";

            // Example: Redirect to the post details page after deleting a comment
            // return RedirectToAction("Details", "Posts", new { id = postIdForRedirection });

            // Or, redirect back to the comment index as per your original code
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a comment exists.
        /// </summary>
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.IdComment == id);
        }
    }
}