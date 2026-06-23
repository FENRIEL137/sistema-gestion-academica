using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Data
{
    public class DataInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = serviceProvider.GetRequiredService<ILogger<DataInitializer>>();

            logger.LogInformation("=== INICIANDO DATA INITIALIZER ===");

            // Crear roles
            string[] roles = { "Administrador", "Empleado", "Profesor", "Estudiante" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole(roleName)
                    {
                        Descripcion = $"Rol de {roleName} del sistema",
                        NormalizedName = roleName.ToUpper()
                    };
                    await roleManager.CreateAsync(role);
                    logger.LogInformation($"Rol creado: {roleName}");
                }
            }

            // Crear admin - USAR DateTime.UtcNow
            var adminEmail = "admin@sistema.edu";
            var adminPassword = "Admin123!";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                logger.LogInformation("Creando usuario administrador...");
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NormalizedUserName = adminEmail.ToUpper(),
                    NormalizedEmail = adminEmail.ToUpper(),
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow,     // CORREGIDO
                    Activo = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                    logger.LogInformation("Usuario administrador creado exitosamente.");
                }
                else
                {
                    logger.LogError($"Error al crear admin: {string.Join(", ", result.Errors)}");
                }
            }

            // Datos iniciales
            if (!context.Aulas.Any())
            {
                context.Aulas.AddRange(
                    new Aula { Codigo = "A101", Nombre = "Aula 101", Capacidad = 30 },
                    new Aula { Codigo = "A102", Nombre = "Aula 102", Capacidad = 30 }
                );
                await context.SaveChangesAsync();
            }

            if (!context.ConfiguracionesInstitucionales.Any())
            {
                context.ConfiguracionesInstitucionales.Add(
                    new ConfiguracionInstitucional
                    {
                        NombreInstitucion = "Instituto de Educacion Superior",
                        PagoInicialInscripcion = 500.00m,
                        CostoBaseMateria = 300.00m,
                        PorcentajePenalizacionMora = 5.00m,
                        DiaInicioPagos = 23,
                        DiaFinPagos = 30,
                        PeriodoActual = "2026-I",
                        FechaUltimaActualizacion = DateTime.UtcNow  // CORREGIDO
                    }
                );
                await context.SaveChangesAsync();
            }

            logger.LogInformation("=== DATA INITIALIZER COMPLETADO ===");
        }
    }
}