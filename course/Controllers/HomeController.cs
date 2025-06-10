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
        const int initialPageSize = 25;

        var posts = await _context.Posts
                                .Where(p => !p.IsHidden)
                                .OrderByDescending(p => p.CreationDate)
                                .Take(initialPageSize)
                                .ToListAsync();

        var authorIds = posts.Select(p => p.IdUser).Distinct().ToList();

        var authorNames = await _context.Users
                                        .Where(u => authorIds.Contains(u.IdUser))
                                        .ToDictionaryAsync(u => u.IdUser, u => u.Login);

        var postIds = posts.Select(p => p.IdPost).ToList();
        var postRatings = await _context.Ratings
                                        .Where(r => r.IdPost.HasValue && postIds.Contains(r.IdPost.Value))
                                        .GroupBy(r => r.IdPost.Value)
                                        .Select(g => new { PostId = g.Key, TotalRating = g.Sum(r => r.Value ? 1 : -1) })
                                        .ToDictionaryAsync(x => x.PostId, x => x.TotalRating);

        var commentCounts = await _context.Comments
                                        .Where(c => postIds.Contains(c.IdPost))
                                        .GroupBy(c => c.IdPost)
                                        .Select(g => new { PostId = g.Key, Count = g.Count() })
                                        .ToDictionaryAsync(x => x.PostId, x => x.Count);

        ViewData["AuthorNames"] = authorNames;
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
