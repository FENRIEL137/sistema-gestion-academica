using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Repositories;
using SistemaGestionAcademica.Services;
using SistemaGestionAcademica.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddControllersWithViews();

builder.Services.AddAntiforgery(options => options.Cookie.Expiration = TimeSpan.Zero);

// =============================================
// BASE DE DATOS
// =============================================
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // RENDER: PostgreSQL
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var portDb = uri.Port > 0 ? uri.Port : 5432;
    var connString = $"Host={uri.Host};Port={portDb};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connString));
}
else
{
    // LOCAL: SQLite (archivo)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=siga.db"));
}

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Repositorios
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IInscripcionService, InscripcionService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicializar
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Creando base de datos...");
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Base de datos lista.");
        await DataInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error: " + ex.Message);
    }
}

app.Run();