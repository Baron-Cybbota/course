using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using course.Models; // Убедитесь, что это пространство имен ваших моделей
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // Для IServiceScopeFactory

namespace course.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

                // Проверяем и создаем роли
                string[] roleNames = { "Пользователь", "Модератор" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new Role { Name = roleName });
                        Console.WriteLine($"Роль '{roleName}' создана.");
                    }
                }

                // Создаем тестового модератора, если его нет
                string moderatorEmail = "moderator@example.com";
                string moderatorPassword = "Password123!"; // Внимание: используйте надежный пароль в реальных проектах!
                string moderatorUserName = "moderator@example.com";

                if (await userManager.FindByEmailAsync(moderatorEmail) == null)
                {
                    var moderator = new User
                    {
                        UserName = moderatorUserName,
                        Email = moderatorEmail,
                        EmailConfirmed = true, // Важно для входа без подтверждения
                        RegistrationDate = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(moderator, moderatorPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(moderator, "Модератор");
                        Console.WriteLine($"Тестовый модератор '{moderatorUserName}' создан.");
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка при создании тестового модератора: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // Убедимся, что существующий пользователь без роли получает роль "Пользователь"
                // (Это опционально, но помогает гарантировать, что все пользователи имеют роль)
                var allUsers = await userManager.Users.ToListAsync();
				foreach (var user in allUsers)
				{
                        Console.WriteLine($"SOSI '{user.Email}' '{user.UserName}'.");

				}
                foreach (var user in allUsers)
					{
						if (!await userManager.IsInRoleAsync(user, "Пользователь") && !await userManager.IsInRoleAsync(user, "Модератор"))
						{
							await userManager.AddToRoleAsync(user, "Пользователь");
							Console.WriteLine($"Пользователю '{user.UserName}' назначена роль 'Пользователь'.");
						}
					}
            }
        }
    }
}