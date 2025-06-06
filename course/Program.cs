using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using course.Data;
using course.Models; // Убедитесь, что здесь есть все ваши модели
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies; // Добавляем для Cookie Authentication
using Microsoft.AspNetCore.Authorization; // Для [AllowAnonymous] и других атрибутов авторизации
using Microsoft.AspNetCore.Authentication; // Для HttpContext.SignInAsync
using System.Security.Claims; // Для Claim

var builder = WebApplication.CreateBuilder(args);

// Добавляем DbContext для ваших моделей
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка аутентификации на основе файлов cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Путь к странице входа
        options.AccessDeniedPath = "/Account/AccessDenied"; // Путь к странице отказа в доступе
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Время жизни куки
        options.SlidingExpiration = true; // Обновлять куки при активности пользователя
    });

// Добавляем сервисы в контейнер.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Применение миграций при запуске приложения
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Применяем миграции при запуске, если они есть
        // Здесь вы можете добавить инициализацию данных, если она необходима
        // await DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Конфигурируем HTTP запрос.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Важно: UseAuthentication() и UseAuthorization() должны быть ПОСЛЕ UseRouting()
// и ПЕРЕД app.MapControllerRoute().
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
