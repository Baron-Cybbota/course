using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models; // Убедитесь, что это пространство имен соответствует вашим моделям
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
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

        // Вспомогательный метод для получения Id текущего пользователя
        private int? GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    return parsedUserId;
                }
            }
            return null;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                                       .Where(p => !p.IsHidden) // Предполагаем, что вы хотите показывать только не скрытые посты
                                       .OrderByDescending(p => p.CreationDate)
                                       .ToListAsync();

            var authorIds = posts.Select(p => p.IdUser).Distinct().ToList();

            var authors = await _context.Users
                                        .Where(u => authorIds.Contains(u.IdUser))
                                        .ToDictionaryAsync(u => u.IdUser, u => u.Login);

            ViewData["Authors"] = authors;
            return View(posts);
        }

        /// <summary>
        /// Возвращает дополнительные посты в виде частичного представления для бесконечной прокрутки.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMorePosts(int pageNumber, int pageSize)
        {
            var posts = await _context.Posts
                                       .OrderByDescending(p => p.CreationDate)
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            if (!posts.Any())
            {
                return Content("");
            }

            var authorIds = posts.Select(p => p.IdUser).Distinct().ToList();

            var authorNames = await _context.Users
                                            .Where(u => authorIds.Contains(u.IdUser))
                                            .ToDictionaryAsync(u => u.IdUser, u => u.Login);

            // Since Rating is now directly on the Post model, we don't need to fetch from Ratings table here.
            // The Post model already has the Rating.
            // var postIds = posts.Select(p => p.IdPost).ToList();
            // var postRatings = await _context.Ratings
            //                                 .Where(r => r.IdPost.HasValue && postIds.Contains(r.IdPost.Value))
            //                                 .GroupBy(r => r.IdPost.Value)
            //                                 .ToDictionaryAsync(g => g.Key, g => g.Sum(r => r.Value ? 1 : -1));

            var commentCounts = await _context.Comments
                                              .Where(c => posts.Select(p => p.IdPost).Contains(c.IdPost)) // Use postIds or directly from posts
                                              .GroupBy(c => c.IdPost)
                                              .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewData["AuthorNames"] = authorNames;
            // ViewData["PostRatings"] = postRatings; // Remove this if you're getting rating from Post.Rating
            ViewData["CommentCounts"] = commentCounts;

            return PartialView("_PostCardPartial", posts);
        }


        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.IdPost == id);
            if (post == null)
            {
                return NotFound();
            }

            var author = await _context.Users.FindAsync(post.IdUser);
            ViewData["AuthorName"] = author?.Login ?? "Неизвестный автор";

            var comments = await _context.Comments
                                         .Where(c => c.IdPost == id)
                                         .OrderBy(c => c.CreationDate)
                                         .ToListAsync();
            ViewData["Comments"] = comments;

            var commentAuthorIds = comments.Select(c => c.IdUser).Distinct().ToList();
            var commentAuthors = await _context.Users
                                               .Where(u => commentAuthorIds.Contains(u.IdUser))
                                               .ToDictionaryAsync(u => u.IdUser, u => u.Login);
            ViewData["CommentAuthors"] = commentAuthors;

            // When Post has Rating property, you fetch it directly from the post object.
            // No need to query Ratings table for post's total rating here.
            // However, you still need to determine if the *current user* has voted.
            // var postRatings = await _context.Ratings.Where(r => r.IdPost == id).ToListAsync();
            // int totalPostRating = postRatings.Sum(r => r.Value ? 1 : -1);
            // ViewData["TotalPostRating"] = totalPostRating; // Use post.Rating instead
            // ViewData["PostRatingsCount"] = postRatings.Count; // This count is still useful for number of votes

            ViewData["TotalPostRating"] = post.Rating; // Use the Rating property directly from the Post model

            int? currentUserId = GetCurrentUserId();
            ViewData["CurrentUserId"] = currentUserId;

            // Check if the current user has rated this post
            if (currentUserId.HasValue)
            {
                var existingPostRating = await _context.Ratings.FirstOrDefaultAsync(r => r.IdUser == currentUserId.Value && r.IdPost == post.IdPost);
                ViewData["ExistingPostRating"] = existingPostRating; // This will tell if user has upvoted/downvoted or not voted
            }


            // DECLARE THESE DICTIONARIES HERE
            var commentTotalRatings = new Dictionary<int, int>();
            var commentRatingsCounts = new Dictionary<int, int>();
            var existingCommentRatings = new Dictionary<int, Rating>(); // Declare for current user's comment ratings

            if (comments != null)
            {
                // Fetch all ratings for comments related to this post in one go to optimize
                var allCommentRatings = await _context.Ratings
                                                        .Where(r => r.IdComment.HasValue && comments.Select(c => c.IdComment).Contains(r.IdComment.Value))
                                                        .ToListAsync();

                foreach (var comment in comments)
                {
                    var currentCommentRatings = allCommentRatings.Where(r => r.IdComment == comment.IdComment).ToList();
                    commentTotalRatings[comment.IdComment] = currentCommentRatings.Sum(r => r.Value ? 1 : -1);
                    commentRatingsCounts[comment.IdComment] = currentCommentRatings.Count;

                    // Check for current user's rating on this specific comment
                    if (currentUserId.HasValue)
                    {
                        var userRatingForComment = currentCommentRatings.FirstOrDefault(r => r.IdUser == currentUserId.Value && r.IdComment == comment.IdComment);
                        if (userRatingForComment != null)
                        {
                            existingCommentRatings[comment.IdComment] = userRatingForComment;
                        }
                    }
                }
            }
            ViewData["CommentTotalRatings"] = commentTotalRatings;
            ViewData["CommentRatingsCounts"] = commentRatingsCounts;
            ViewData["ExistingCommentRatings"] = existingCommentRatings; // Pass this to the view

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,IsHidden,IdEvent")] Post post)
        {
            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Вы должны быть залогинены, чтобы создать пост.");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                post.IdUser = currentUserId.Value;
                post.CreationDate = DateTime.UtcNow;
                post.EditDate = null;
                post.Rating = 0; // Initialize rating to 0 when creating a new post

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue || post.IdUser != currentUserId.Value)
            {
                return Forbid();
            }

            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPost,Title,Content,IsHidden,IdEvent")] Post post)
        {
            if (id != post.IdPost)
            {
                return NotFound();
            }

            int? currentUserId = GetCurrentUserId();

            var existingPost = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.IdPost == id);
            if (existingPost == null)
            {
                return NotFound();
            }

            if (!currentUserId.HasValue || existingPost.IdUser != currentUserId.Value)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.EditDate = DateTime.UtcNow;
                    post.CreationDate = existingPost.CreationDate;
                    post.IdUser = existingPost.IdUser;
                    post.Rating = existingPost.Rating; // Preserve the existing rating

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.IdPost))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = post.IdPost });
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.IdPost == id);
            if (post == null)
            {
                return NotFound();
            }

            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue || post.IdUser != currentUserId.Value)
            {
                return Forbid();
            }

            var author = await _context.Users.FindAsync(post.IdUser);
            ViewData["AuthorName"] = author?.Login ?? "Неизвестный автор";

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue || post.IdUser != currentUserId.Value)
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Новый экшен для обработки голосования
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int postId, string voteType)
        {
            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Вы должны быть залогинены, чтобы голосовать." });
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Пост не найден." });
            }

            var existingRating = await _context.Ratings
                                               .FirstOrDefaultAsync(r => r.IdUser == currentUserId.Value && r.IdPost == postId);

            bool isUpvote = (voteType == "up");
            int voteValue = isUpvote ? 1 : -1;

            if (existingRating == null)
            {
                // Пользователь еще не голосовал за этот пост, добавляем новую запись
                var newRating = new Rating
                {
                    IdUser = currentUserId.Value,
                    IdPost = postId,
                    Value = isUpvote,
                };
                _context.Ratings.Add(newRating);
                post.Rating += voteValue; // Update Post.Rating
            }
            else
            {
                // Пользователь уже голосовал
                if (existingRating.Value == isUpvote)
                {
                    // Голос тот же, что и был - отменяем голос (удаляем запись)
                    _context.Ratings.Remove(existingRating);
                    post.Rating -= voteValue; // Update Post.Rating
                }
                else
                {
                    // Голос противоположный - меняем голос
                    existingRating.Value = isUpvote;
                    _context.Ratings.Update(existingRating);
                    post.Rating += (voteValue * 2); // Update Post.Rating: -1 becomes 1 (+2), 1 becomes -1 (-2)
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, newRating = post.Rating }); // Return the updated post.Rating
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Ошибка сохранения данных. Пожалуйста, попробуйте снова." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error voting: {ex.Message}");
                return Json(new { success = false, message = "Произошла непредвиденная ошибка." });
            }
        }


        // Вспомогательный метод для проверки существования поста
        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.IdPost == id);
        }
    }
}