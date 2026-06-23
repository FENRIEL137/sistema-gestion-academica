using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Repositories;
using SistemaGestionAcademica.Services;
using SistemaGestionAcademica.Services.Interfaces;

static string ConvertPostgresUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
    {
        var npgsqlConnectionString = ConvertPostgresUrlToConnectionString(connectionString);
        options.UseNpgsql(npgsqlConnectionString,
            npgsqlOptions => npgsqlOptions.MigrationsAssembly("SistemaGestionAcademica"));
    }
    else
    {
        options.UseSqlServer(connectionString,
            sqlOptions => sqlOptions.MigrationsAssembly("SistemaGestionAcademica"));
    }
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IAulaRepository, AulaRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();
builder.Services.AddScoped<IInscripcionRepository, InscripcionRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IActividadRepository, ActividadRepository>();
builder.Services.AddScoped<INotaRepository, NotaRepository>();
builder.Services.AddScoped<IConfiguracionInstitucionalRepository, ConfiguracionInstitucionalRepository>();
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
app.UseHttpsRedirection();
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

// =============================================
// CREAR TABLAS DIRECTAMENTE CON SQL
// =============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Ejecutando EnsureCreated...");
        await context.Database.EnsureCreatedAsync();

        // Verificar si la tabla AspNetUsers existe
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS ""AspNetUsers"" (
                ""Id"" text NOT NULL PRIMARY KEY,
                ""UserName"" text,
                ""NormalizedUserName"" text,
                ""Email"" text,
                ""NormalizedEmail"" text,
                ""EmailConfirmed"" boolean NOT NULL DEFAULT FALSE,
                ""PasswordHash"" text,
                ""SecurityStamp"" text,
                ""ConcurrencyStamp"" text,
                ""PhoneNumber"" text,
                ""PhoneNumberConfirmed"" boolean NOT NULL DEFAULT FALSE,
                ""TwoFactorEnabled"" boolean NOT NULL DEFAULT FALSE,
                ""LockoutEnd"" timestamp with time zone,
                ""LockoutEnabled"" boolean NOT NULL DEFAULT TRUE,
                ""AccessFailedCount"" integer NOT NULL DEFAULT 0,
                ""NombreCompleto"" text NOT NULL DEFAULT '',
                ""FechaRegistro"" timestamp with time zone NOT NULL DEFAULT NOW(),
                ""Activo"" boolean NOT NULL DEFAULT TRUE,
                ""UltimoAcceso"" timestamp with time zone
            );
            
            CREATE TABLE IF NOT EXISTS ""AspNetRoles"" (
                ""Id"" text NOT NULL PRIMARY KEY,
                ""Name"" text,
                ""NormalizedName"" text,
                ""ConcurrencyStamp"" text,
                ""Descripcion"" text,
                ""FechaCreacion"" timestamp with time zone NOT NULL DEFAULT NOW()
            );
            
            CREATE TABLE IF NOT EXISTS ""AspNetUserRoles"" (
                ""UserId"" text NOT NULL,
                ""RoleId"" text NOT NULL,
                PRIMARY KEY (""UserId"", ""RoleId""),
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles""(""Id"") ON DELETE CASCADE
            );
            
            CREATE TABLE IF NOT EXISTS ""AspNetUserClaims"" (
                ""Id"" serial PRIMARY KEY,
                ""UserId"" text NOT NULL,
                ""ClaimType"" text,
                ""ClaimValue"" text,
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE
            );
            
            CREATE TABLE IF NOT EXISTS ""AspNetRoleClaims"" (
                ""Id"" serial PRIMARY KEY,
                ""RoleId"" text NOT NULL,
                ""ClaimType"" text,
                ""ClaimValue"" text,
                FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles""(""Id"") ON DELETE CASCADE
            );
            
            CREATE TABLE IF NOT EXISTS ""AspNetUserLogins"" (
                ""LoginProvider"" text NOT NULL,
                ""ProviderKey"" text NOT NULL,
                ""ProviderDisplayName"" text,
                ""UserId"" text NOT NULL,
                PRIMARY KEY (""LoginProvider"", ""ProviderKey""),
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE
            );
            
            CREATE TABLE IF NOT EXISTS ""AspNetUserTokens"" (
                ""UserId"" text NOT NULL,
                ""LoginProvider"" text NOT NULL,
                ""Name"" text NOT NULL,
                ""Value"" text,
                PRIMARY KEY (""UserId"", ""LoginProvider"", ""Name""),
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE
            );
        ";

        await command.ExecuteNonQueryAsync();
        logger.LogInformation("Tablas de Identity creadas exitosamente.");

        await DataInitializer.InitializeAsync(services);
        logger.LogInformation("Inicializacion completada.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error: " + ex.Message);
        if (ex.InnerException != null)
        {
            logger.LogError(ex.InnerException, "Error interno: " + ex.InnerException.Message);
        }
    }
}

app.Run();