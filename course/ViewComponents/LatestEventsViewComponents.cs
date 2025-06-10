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
            // Ensure to handle nullable IdBoardGame (using .Where(e => e.IdBoardGame.HasValue))
            var boardGameIds = events.Where(e => e.IdBoardGame.HasValue) // Only include events that actually have a board game linked
                                     .Select(e => e.IdBoardGame.Value) // Get the non-nullable int value
                                     .Distinct()
                                     .ToList();

            var boardGameTitles = await _context.BoardGames
                                                 .Where(bg => boardGameIds.Contains(bg.IdBoardGame))
                                                 .ToDictionaryAsync(bg => bg.IdBoardGame, bg => bg.Name);

            // Create a ViewModel or an anonymous type to pass both events and titles
            var viewModel = events.Select(e => new
            {
                Event = e,
                // CORRECTED: Use e.IdBoardGame instead of e.BoardGameId
                // Also, account for nullable IdBoardGame in GetValueOrDefault
                BoardGameTitle = e.IdBoardGame.HasValue
                               ? boardGameTitles.GetValueOrDefault(e.IdBoardGame.Value, "Неизвестная игра")
                               : "Без настольной игры" // Or any other placeholder if no game is linked
            }).ToList();

            return View(viewModel);
        }
    }
}