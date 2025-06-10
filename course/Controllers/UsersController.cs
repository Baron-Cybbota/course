using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using course.Models.ViewModels;
using System.Threading.Tasks; // For Task
using System; // For DateTime
using System.Linq; // For Any()
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList (though not directly used in this controller anymore after AccessLevel removal)

namespace course.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        // Displays a list of all users.
        public async Task<IActionResult> Index()
        {
            // This is correct. ToListAsync() works on the DbSet.
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        // Displays details for a specific user by Id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.IdUser == id); // Changed m.Id to m.IdUser
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        // Displays the form for creating a new user.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // Processes the form submission for creating a new user.
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection
        // --- CHANGE REQUIRED ---
        // User.Id is now User.IdUser, so it won't be bound here unless explicitly added and generated.
        // It's an identity column, so it shouldn't be bound.
        // Removed AccessLevel from User model, so it's not in the bind.
        public async Task<IActionResult> Create([Bind("Login,Email,PasswordHash,BlockStatus,Rating")] User user)
        {
            if (ModelState.IsValid) // Check model validity based on Data Annotations
            {
                // In a real application:
                // 1. Password must be hashed (e.g., using BCrypt.Net-Core or AspNetCore.Cryptography.KeyDerivation)
                // user.PasswordHash = YourPasswordHasher.HashPassword(user.PasswordHash);

                // Set registration date
                user.RegistrationDate = DateTime.UtcNow;

                _context.Add(user); // Add user to context
                await _context.SaveChangesAsync(); // Save changes to the database
                TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно создан.";
                return RedirectToAction(nameof(Index)); // Redirect to user list
            }
            TempData["ErrorMessage"] = "Ошибка при создании пользователя.";
            return View(user); // If model is invalid, return form with errors
        }

        // GET: Users/Edit/5
        // Displays the form for editing an existing user.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser. FindAsync should use IdUser.
            var user = await _context.Users.FindAsync(id); // FindAsync works with the primary key
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // Processes the form submission for editing a user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // --- CHANGE REQUIRED ---
        // User.Id is now User.IdUser. Use IdUser in Bind.
        // Also removed AccessLevel from User model, so it's not in the bind.
        public async Task<IActionResult> Edit(int id, [Bind("IdUser,Login,Email,PasswordHash,RegistrationDate,BlockStatus,Rating")] User user) // Changed Id to IdUser
        {
            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser. Check against IdUser.
            if (id != user.IdUser) // Check if Id from route matches model IdUser
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Caution: if you allow PasswordHash to be edited via this form,
                    // it needs to be hashed again if it has changed.
                    // Or create a separate action for password changes.

                    // If RegistrationDate is not sent from the form, you might need to preserve it:
                    // var originalUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdUser == user.IdUser);
                    // if (originalUser != null)
                    // {
                    //     user.RegistrationDate = originalUser.RegistrationDate;
                    // }

                    _context.Update(user); // Update user in context
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException) // Handle concurrency conflicts
                {
                    // --- CHANGE REQUIRED ---
                    // User.Id is now User.IdUser. Check against IdUser.
                    if (!UserExists(user.IdUser)) // Changed user.Id to user.IdUser
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throw error if it's not a concurrency conflict
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Ошибка при обновлении пользователя.";
            return View(user);
        }

        // GET: Users/Delete/5
        // Displays the user deletion confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser.
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.IdUser == id); // Changed m.Id to m.IdUser
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        // Processes the user deletion confirmation.
        [HttpPost, ActionName("Delete")] // Specify this is a POST action for the Delete route
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser. FindAsync should use IdUser.
            var user = await _context.Users.FindAsync(id); // FindAsync works with primary key (IdUser)
            if (user != null)
            {
                _context.Users.Remove(user); // Remove user from context
            }

            await _context.SaveChangesAsync(); // Save changes
            TempData["SuccessMessage"] = "Пользователь успешно удален.";
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a user exists
        private bool UserExists(int id)
        {
            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser.
            return _context.Users.Any(e => e.IdUser == id); // Changed e.Id to e.IdUser
        }

        // =====================================================================
        // NEW ACTIONS FOR MANAGING ADMINISTRATOR STATUS
        // =====================================================================

        // GET: Users/ManageAdministratorStatus/5
        // Displays the form for managing a specific user's administrator status.
        public async Task<IActionResult> ManageAdministratorStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser. FindAsync should use IdUser.
            var user = await _context.Users.FindAsync(id); // FindAsync works with primary key (IdUser)
            if (user == null)
            {
                return NotFound();
            }

            // --- CHANGE REQUIRED ---
            // Administrator.UserId is now Administrator.IdUser.
            var administrator = await _context.Administrators.FirstOrDefaultAsync(a => a.IdUser == id); // Changed a.UserId to a.IdUser

            var viewModel = new UserAdminStatusViewModel
            {
                // --- CHANGE REQUIRED ---
                // User.Id is now User.IdUser.
                UserId = user.IdUser, // Changed user.Id to user.IdUser
                UserName = user.Login,
                Email = user.Email,
                IsAdministrator = (administrator != null),
                // AccessLevel is removed from Administrator model, so no longer needed here. This is correct.
                // AccessLevel = administrator?.AccessLevel,
                AssignmentDate = administrator?.AssignmentDate
            };

            return View(viewModel);
        }

        // POST: Users/ManageAdministratorStatus/5
        // Processes the form submission for managing administrator status.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Remove AccessLevel from Bind attribute as it's no longer in the model. This is correct.
        public async Task<IActionResult> ManageAdministratorStatus(int id, [Bind("UserId,IsAdministrator")] UserAdminStatusViewModel viewModel)
        {
            // --- CHANGE REQUIRED ---
            // viewModel.UserId is expected to be the IdUser.
            if (id != viewModel.UserId)
            {
                return NotFound();
            }

            // Get original user data to ensure Login and Email weren't tampered with
            // --- CHANGE REQUIRED ---
            // User.Id is now User.IdUser. FindAsync should use IdUser.
            var user = await _context.Users.FindAsync(id); // FindAsync works with primary key (IdUser)
            if (user == null)
            {
                return NotFound();
            }

            // Add these values back to viewModel for correct display in case of error
            viewModel.UserName = user.Login;
            viewModel.Email = user.Email;

            // Since AccessLevel is removed, the validation logic related to it also needs to be removed. This is correct.
            // if (viewModel.IsAdministrator && (viewModel.AccessLevel == null || viewModel.AccessLevel <= 0))
            // {
            //     ModelState.AddModelError("AccessLevel", "Для администратора необходимо указать уровень доступа больше 0.");
            // }
            // else if (!viewModel.IsAdministrator)
            // {
            //     viewModel.AccessLevel = null;
            // }

            if (ModelState.IsValid)
            {
                // --- CHANGE REQUIRED ---
                // Administrator.UserId is now Administrator.IdUser.
                var existingAdmin = await _context.Administrators.FirstOrDefaultAsync(a => a.IdUser == id); // Changed a.UserId to a.IdUser

                if (viewModel.IsAdministrator)
                {
                    // If the user should be an administrator
                    if (existingAdmin == null)
                    {
                        // Create a new Administrator entry
                        var newAdmin = new Administrator
                        {
                            // --- CHANGE REQUIRED ---
                            // Administrator.UserId is now Administrator.IdUser.
                            IdUser = viewModel.UserId, // Changed UserId to IdUser
                            // AccessLevel is removed from Administrator model. This is correct.
                            // AccessLevel = viewModel.AccessLevel ?? 1, // Use provided AccessLevel or default to 1
                            AssignmentDate = DateTime.UtcNow
                        };
                        _context.Administrators.Add(newAdmin);
                        TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно назначен администратором.";
                    }
                    // Since AccessLevel is removed, there's no update logic for it. This is correct.
                    // else
                    // {
                    //     // Update existing Administrator entry
                    //     // existingAdmin.AccessLevel = viewModel.AccessLevel ?? existingAdmin.AccessLevel; // Update access level
                    //     // _context.Administrators.Update(existingAdmin);
                    //     // TempData["SuccessMessage"] = $"Уровень доступа администратора '{user.Login}' успешно обновлен.";
                    // }
                }
                else
                {
                    // If the user should NO LONGER be an administrator
                    if (existingAdmin != null)
                    {
                        // Remove the Administrator entry
                        _context.Administrators.Remove(existingAdmin);
                        TempData["SuccessMessage"] = $"Пользователь '{user.Login}' успешно лишен статуса администратора.";
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "Users"); // Redirect to user list
            }

            // If ModelState is not valid, re-display the view with errors
            // Pass the current assignment date, if it existed (for re-displaying the form)
            // --- CHANGE REQUIRED ---
            // Administrator.UserId is now Administrator.IdUser.
            var currentAdmin = await _context.Administrators.AsNoTracking().FirstOrDefaultAsync(a => a.IdUser == id); // Changed a.UserId to a.IdUser
            viewModel.AssignmentDate = currentAdmin?.AssignmentDate;
            return View(viewModel);
        }
    }
}