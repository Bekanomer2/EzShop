using EzShop.BLL.Interfaces;
using EzShop.BLL.Services;
using EzShop.DAL.Context;
using EzShop.DAL.Repositories;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение к облачной базе данных PostgreSQL (Supabase)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.vsxvbrjfuhzvdyzbpjmn;Password=BekaShop2026;Ssl Mode=Require;Trust Server Certificate=true;Timeout=5;Command Timeout=5";

builder.Services.AddDbContext<EzShopDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.CommandTimeout(5);
    });
});

// 2. Настройка ASP.NET Core Identity с целочисленными ключами IdentityRole<int>, User : IdentityUser<int>
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<EzShopDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "EzShop.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// 3. Регистрация паттерна Репозиторий (DAL -> Domain.Interfaces)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// 4. Регистрация сервисов бизнес-логики (BLL -> BLL.Interfaces)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICartService, CartService>();

// 5. Внешние сервисы (Telegram Bot)
builder.Services.AddScoped<ITelegramService, TelegramService>();

// 6. MVC контроллеры и представления
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
builder.Services.AddControllersWithViews();

// Поддержка динамического порта для облачных хостингов (Render / Railway)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// Инициализация дефолтных данных (если таблицы в БД пусты)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<EzShopDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await AdminInitializer.SeedAdminAndRoleDataAsync(roleManager, userManager);
        await DbInitializer.SeedProductsAndBrandsAsync(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Ошибка сидинга БД: {Message}", ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Catalog/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Аутентификация и авторизация Identity
app.UseAuthentication();
app.UseAuthorization();

// Лёгкий эндпоинт для мониторинга и поддержания активности (UptimeRobot / Render)
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow })).AllowAnonymous();

// Стандартный маршрут MVC (все остальные прямые маршруты и .html настроены через атрибуты [Route] в контроллерах)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
