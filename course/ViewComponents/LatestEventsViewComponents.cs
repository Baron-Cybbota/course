using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // For Dictionary

namespace course.ViewComponents
{
    public class LatestEventsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public LatestEventsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Fetch the upcoming events, ordered by date and time, taking the latest 5
            var events = await _context.Events
                                       .OrderByDescending(e => e.Date)
                                       .ThenByDescending(e => e.Time)
                                       .Take(5)
                                       .ToListAsync();

            // Fetch the board game titles for these events
            var boardGameIds = events.Select(e => e.BoardGameId).Distinct().ToList();
            var boardGameTitles = await _context.BoardGames
                                                .Where(bg => boardGameIds.Contains(bg.Id))
                                                .ToDictionaryAsync(bg => bg.Id, bg => bg.Title);

            // Create a ViewModel or an anonymous type to pass both events and titles
            var viewModel = events.Select(e => new
            {
                Event = e,
                BoardGameTitle = boardGameTitles.GetValueOrDefault(e.BoardGameId, "Неизвестная игра")
            }).ToList();

            return View(viewModel);
        }
    }
}
