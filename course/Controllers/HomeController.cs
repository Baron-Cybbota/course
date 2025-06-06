using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Для Dictionary
using System.Diagnostics;

namespace course.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                                       .Where(p => !p.IsHidden)
                                       .OrderByDescending(p => p.CreationDate)
                                       .ToListAsync();

            // Получаем все уникальные AuthorId из полученных постов
            var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();

            // Загружаем пользователей (авторов) по их ID напрямую из DbContext
            var authors = await _context.Users
                                        .Where(u => authorIds.Contains(u.Id))
                                        .ToDictionaryAsync(u => u.Id, u => u.Login);

            ViewData["AuthorNames"] = authors;

            // Вычисляем суммарный рейтинг и количество комментариев для каждого поста
            var postRatings = await _context.Ratings
                                            .Where(r => r.PostId.HasValue && posts.Select(p => p.Id).Contains(r.PostId.Value))
                                            .GroupBy(r => r.PostId.Value)
                                            .Select(g => new { PostId = g.Key, TotalRating = g.Sum(r => r.Value ? 1 : -1) })
                                            .ToDictionaryAsync(x => x.PostId, x => x.TotalRating);

            var commentCounts = await _context.Comments
                                              .Where(c => posts.Select(p => p.Id).Contains(c.PostId))
                                              .GroupBy(c => c.PostId)
                                              .Select(g => new { PostId = g.Key, Count = g.Count() })
                                              .ToDictionaryAsync(x => x.PostId, x => x.Count);

            ViewData["PostRatings"] = postRatings;
            ViewData["CommentCounts"] = commentCounts;

            return View(posts);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
