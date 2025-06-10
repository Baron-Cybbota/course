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
    public class BlacklistEntriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlacklistEntriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BlacklistEntries
        // Отображает список всех записей в черном списке.
        public async Task<IActionResult> Index()
        {
            // Instead of .Include(), we will join explicitly to get related data.
            var blacklistEntriesWithDetails = await _context.BlacklistEntries
                .Select(b => new
                {
                    BlacklistEntry = b,
                    UserName = _context.Users.Where(u => u.IdUser == b.IdUser).Select(u => u.Login).FirstOrDefault(),
                    AdministratorLogin = _context.Administrators
                                            .Where(a => a.IdAdministrator == b.IdAdministrator)
                                            .Join(_context.Users, // Join Administrators with Users
                                                  a => a.IdUser, // Administrator's foreign key to User
                                                  u => u.IdUser, // User's primary key
                                                  (a, u) => u.Login) // Select User's Login
                                            .FirstOrDefault()
                })
                .ToListAsync();

            // Prepare dictionaries for View
            var userLogins = new Dictionary<int, string>();
            var administratorLogins = new Dictionary<int, string>();

            foreach (var item in blacklistEntriesWithDetails)
            {
                // Populate dictionaries, handling cases where ID might not exist (though for Required FKs, they should)
                if (item.BlacklistEntry.IdUser != 0 && item.UserName != null) // Assuming IdUser is not 0 for valid entries
                {
                    userLogins[item.BlacklistEntry.IdUser] = item.UserName;
                }
                if (item.BlacklistEntry.IdAdministrator != 0 && item.AdministratorLogin != null)
                {
                    administratorLogins[item.BlacklistEntry.IdAdministrator] = item.AdministratorLogin;
                }
            }

            ViewData["UserLogins"] = userLogins;
            ViewData["AdministratorLogins"] = administratorLogins;

            // Pass the original list of BlacklistEntry objects to the view
            // The view will then use ViewData to display the related names
            return View(blacklistEntriesWithDetails.Select(x => x.BlacklistEntry).ToList());
        }


        // GET: BlacklistEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blacklistEntry = await _context.BlacklistEntries
                .FirstOrDefaultAsync(m => m.IdBlacklist == id); // Corrected: Use IdBlacklist
            if (blacklistEntry == null)
            {
                return NotFound();
            }

            // Fetch related logins without navigation properties
            var adminLogin = await _context.Administrators
                                           .Where(a => a.IdAdministrator == blacklistEntry.IdAdministrator)
                                           .Join(_context.Users,
                                                 a => a.IdUser,
                                                 u => u.IdUser,
                                                 (a, u) => u.Login)
                                           .FirstOrDefaultAsync();

            var userLogin = await _context.Users
                                          .Where(u => u.IdUser == blacklistEntry.IdUser)
                                          .Select(u => u.Login)
                                          .FirstOrDefaultAsync();

            ViewData["AdministratorLogin"] = adminLogin ?? "Неизвестный администратор";
            ViewData["UserLogin"] = userLogin ?? "Неизвестный пользователь";

            return View(blacklistEntry);
        }

        // GET: BlacklistEntries/Create
        public async Task<IActionResult> Create()
        {
            // For selecting AdministratorId: list of administrators (their IdAdministrator and logins)
            var administratorsForSelectList = await _context.Administrators
                                                            .Join(_context.Users,
                                                                  a => a.IdUser,
                                                                  u => u.IdUser,
                                                                  (a, u) => new { a.IdAdministrator, Login = $"{u.Login} (ID: {a.IdAdministrator})" })
                                                            .ToListAsync();
            ViewBag.Administrators = new SelectList(administratorsForSelectList, "IdAdministrator", "Login"); // Corrected: Use IdAdministrator

            // For selecting IdUser for blocking: list of all users who are not yet blocked
            var blockedUserIds = await _context.BlacklistEntries.Select(b => b.IdUser).ToListAsync(); // Corrected: Use IdUser
            var availableUsers = await _context.Users
                                                .Where(u => !blockedUserIds.Contains(u.IdUser)) // Corrected: Use IdUser
                                                .Select(u => new { u.IdUser, Text = $"{u.Login} (ID: {u.IdUser})" }) // Corrected: Use IdUser
                                                .ToListAsync();
            ViewBag.Users = new SelectList(availableUsers, "IdUser", "Text"); // Corrected: Use IdUser

            return View();
        }

        // POST: BlacklistEntries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAdministrator,IdUser,Reason,BlockDuration")] BlacklistEntry blacklistEntry)
        {
            if (await _context.BlacklistEntries.AnyAsync(b => b.IdUser == blacklistEntry.IdUser))
            {
                ModelState.AddModelError("IdUser", "Этот пользователь уже находится в черном списке.");
            }

            if (ModelState.IsValid)
            {
                blacklistEntry.BlockDate = DateTime.UtcNow;

                _context.Add(blacklistEntry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Запись в черный список успешно добавлена.";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate ViewBag on validation error
            var administratorsForSelectList = await _context.Administrators
                                                            .Join(_context.Users,
                                                                  a => a.IdUser,
                                                                  u => u.IdUser,
                                                                  (a, u) => new { a.IdAdministrator, Login = $"{u.Login} (ID: {a.IdAdministrator})" })
                                                            .ToListAsync();
            ViewBag.Administrators = new SelectList(administratorsForSelectList, "IdAdministrator", "Login", blacklistEntry.IdAdministrator);

            var blockedUserIds = await _context.BlacklistEntries.Select(b => b.IdUser).ToListAsync();
            var availableUsers = await _context.Users
                                                .Where(u => !blockedUserIds.Contains(u.IdUser) || u.IdUser == blacklistEntry.IdUser)
                                                .Select(u => new { u.IdUser, Text = $"{u.Login} (ID: {u.IdUser})" })
                                                .ToListAsync();
            ViewBag.Users = new SelectList(availableUsers, "IdUser", "Text", blacklistEntry.IdUser);
            TempData["ErrorMessage"] = "Ошибка при создании записи. Проверьте введенные данные.";

            return View(blacklistEntry);
        }

        // GET: BlacklistEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blacklistEntry = await _context.BlacklistEntries.FindAsync(id);
            if (blacklistEntry == null)
            {
                return NotFound();
            }

            var administratorsForSelectList = await _context.Administrators
                                                            .Join(_context.Users,
                                                                  a => a.IdUser,
                                                                  u => u.IdUser,
                                                                  (a, u) => new { a.IdAdministrator, Login = $"{u.Login} (ID: {a.IdAdministrator})" })
                                                            .ToListAsync();
            ViewBag.Administrators = new SelectList(administratorsForSelectList, "IdAdministrator", "Login", blacklistEntry.IdAdministrator);

            var blockedUser = await _context.Users.FindAsync(blacklistEntry.IdUser);
            ViewData["UserLogin"] = blockedUser?.Login ?? "Неизвестный пользователь";

            return View(blacklistEntry);
        }

        // POST: BlacklistEntries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdBlacklist,IdAdministrator,Reason,BlockDuration")] BlacklistEntry blacklistEntry)
        {
            if (id != blacklistEntry.IdBlacklist)
            {
                return NotFound();
            }

            var existingBlacklistEntry = await _context.BlacklistEntries.AsNoTracking().FirstOrDefaultAsync(b => b.IdBlacklist == id);
            if (existingBlacklistEntry == null)
            {
                return NotFound();
            }

            blacklistEntry.IdUser = existingBlacklistEntry.IdUser;
            blacklistEntry.BlockDate = existingBlacklistEntry.BlockDate;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blacklistEntry);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Запись в черный список успешно обновлена.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlacklistEntryExists(blacklistEntry.IdBlacklist))
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

            // Repopulate ViewBag on validation error
            var administratorsForSelectList = await _context.Administrators
                                                            .Join(_context.Users,
                                                                  a => a.IdUser,
                                                                  u => u.IdUser,
                                                                  (a, u) => new { a.IdAdministrator, Login = $"{u.Login} (ID: {a.IdAdministrator})" })
                                                            .ToListAsync();
            ViewBag.Administrators = new SelectList(administratorsForSelectList, "IdAdministrator", "Login", blacklistEntry.IdAdministrator);

            var blockedUser = await _context.Users.FindAsync(blacklistEntry.IdUser);
            ViewData["UserLogin"] = blockedUser?.Login ?? "Неизвестный пользователь";
            TempData["ErrorMessage"] = "Ошибка при обновлении записи. Проверьте введенные данные.";

            return View(blacklistEntry);
        }

        // GET: BlacklistEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blacklistEntry = await _context.BlacklistEntries
                .FirstOrDefaultAsync(m => m.IdBlacklist == id);
            if (blacklistEntry == null)
            {
                return NotFound();
            }

            // Fetch related logins explicitly
            var adminLogin = await _context.Administrators
                                           .Where(a => a.IdAdministrator == blacklistEntry.IdAdministrator)
                                           .Join(_context.Users,
                                                 a => a.IdUser,
                                                 u => u.IdUser,
                                                 (a, u) => u.Login)
                                           .FirstOrDefaultAsync();
            var userLogin = await _context.Users
                                          .Where(u => u.IdUser == blacklistEntry.IdUser)
                                          .Select(u => u.Login)
                                          .FirstOrDefaultAsync();

            ViewData["AdministratorLogin"] = adminLogin ?? "Неизвестный администратор";
            ViewData["UserLogin"] = userLogin ?? "Неизвестный пользователь";

            return View(blacklistEntry);
        }

        // POST: BlacklistEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blacklistEntry = await _context.BlacklistEntries.FindAsync(id);
            if (blacklistEntry != null)
            {
                _context.BlacklistEntries.Remove(blacklistEntry);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Запись в черный список успешно удалена.";
            return RedirectToAction(nameof(Index));
        }

        private bool BlacklistEntryExists(int id)
        {
            return _context.BlacklistEntries.Any(e => e.IdBlacklist == id);
        }
    }
}