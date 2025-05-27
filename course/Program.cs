// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Добавляем using для Identity
using Npgsql.EntityFrameworkCore.PostgreSQL;
using course.Data;
using course.Models; // Добавляем using для наших моделей User и Role

var builder = WebApplication.CreateBuilder(args);

// Добавляем DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<courseIdentityDbContext>();

// Добавляем Identity
// .AddDefaultIdentity<User>(...) - это упрощенный способ настройки Identity.
// Он автоматически добавляет UserStore, SignInManager и т.д.
// .AddRoles<Role>() - добавляет поддержку ролей.
// .AddEntityFrameworkStores<ApplicationDbContext>() - указывает, что Identity будет использовать EF Core и наш ApplicationDbContext.
// .AddDefaultTokenProviders() - добавляет провайдеры для генерации токенов (например, для сброса пароля).
builder.Services.AddIdentity<User, Role>(options =>
{
    // Опции для пароля
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 0; // Для простоты в начале

    // Опции для Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Опции для User
    options.User.RequireUniqueEmail = true; // Важно: Email должен быть уникальным
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Добавляем поддержку Razor Pages для Identity UI

var app = builder.Build();

// Configure the HTTP request pipeline.
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
// и ПЕРЕД app.MapControllerRoute()
app.UseAuthentication();
app.UseAuthorization();

// Используйте Razor Pages для Identity UI
app.MapRazorPages(); // Добавьте это для маршрутизации страниц Identity

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();