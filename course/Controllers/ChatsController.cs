using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using System.Threading.Tasks; // For Task
using System; // For DateTime
using System.Linq; // For Any()

namespace course.Controllers
{
    public class ChatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Chats
        // Displays a list of all chats.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Chats.ToListAsync());
        }

        // GET: Chats/Details/5
        // Displays the details of a specific chat by Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // If Id is not provided, return 404
            }

            var chat = await _context.Chats
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chat == null)
            {
                return NotFound(); // If chat is not found, return 404
            }

            return View(chat);
        }

        // GET: Chats/Create
        // Displays the form for creating a new chat.
        public async Task<IActionResult> Create()
        {
            // If you need to select a CreatorId from a list of users,
            // you might pass available users via ViewBag or a ViewModel.
            // For example:
            // ViewBag.CreatorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login");
            return View();
        }

        // POST: Chats/Create
        // Handles form submission for creating a new chat.
        [HttpPost]
        [ValidateAntiForgeryToken] // Protection against CSRF attacks
        public async Task<IActionResult> Create([Bind("CreatorId,Name")] Chat chat)
        {
            if (ModelState.IsValid) // Checks model validity based on Data Annotations
            {
                chat.CreationDate = DateTime.UtcNow; // Set the creation date
                
                _context.Add(chat); // Add the chat to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                return RedirectToAction(nameof(Index)); // Redirect to the chat list
            }
            // If the model is invalid, return the form with errors
            // If you used ViewBag for CreatorId, re-add it here:
            // ViewBag.CreatorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chat.CreatorId);
            return View(chat);
        }

        // GET: Chats/Edit/5
        // Displays the form for editing an existing chat.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats.FindAsync(id); // Find the chat by Id
            if (chat == null)
            {
                return NotFound();
            }
            // If you need to select a CreatorId, re-add it here:
            // ViewBag.CreatorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chat.CreatorId);
            return View(chat);
        }

        // POST: Chats/Edit/5
        // Handles form submission for editing a chat.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind("Id,CreatorId,Name,CreationDate") - ensure all fields that can be changed are included.
        // CreationDate is typically not edited.
        public async Task<IActionResult> Edit(int id, [Bind("Id,CreatorId,Name,CreationDate")] Chat chat)
        {
            if (id != chat.Id) // Check if the Id from the route matches the model's Id
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chat); // Update the chat in the context
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // Handle concurrency conflicts
                {
                    if (!ChatExists(chat.Id))
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
            // If you used ViewBag for CreatorId, re-add it here:
            // ViewBag.CreatorId = new SelectList(await _context.Users.ToListAsync(), "Id", "Login", chat.CreatorId);
            return View(chat);
        }

        // GET: Chats/Delete/5
        // Displays the chat deletion confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chat == null)
            {
                return NotFound();
            }

            return View(chat);
        }

        // POST: Chats/Delete/5
        // Handles the confirmation of chat deletion.
        [HttpPost, ActionName("Delete")] // Specifies that this is the POST action for the Delete route
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat != null)
            {
                _context.Chats.Remove(chat); // Remove the chat from the context
            }
            
            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a chat exists
        private bool ChatExists(int id)
        {
            return _context.Chats.Any(e => e.Id == id);
        }
    }
}
