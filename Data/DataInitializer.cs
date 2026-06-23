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

            // =============================================
            // 1. CREAR ROLES
            // =============================================
            string[] roles = { "Administrador", "Empleado", "Profesor", "Estudiante" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                {
                    var roleResult = await roleManager.CreateAsync(new ApplicationRole(r));
                    if (roleResult.Succeeded)
                        logger.LogInformation($"Rol creado: {r}");
                    else
                        logger.LogError($"Error al crear rol {r}: {string.Join(", ", roleResult.Errors)}");
                }
                else
                {
                    logger.LogInformation($"Rol ya existe: {r}");
                }
            }

            // =============================================
            // 2. CREAR ADMIN
            // =============================================
            var email = "admin@sistema.edu";
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                logger.LogInformation("Creando admin...");
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NormalizedUserName = email.ToUpper(),
                    NormalizedEmail = email.ToUpper(),
                    NombreCompleto = "Administrador",
                    EmailConfirmed = true,
                    Activo = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, "Admin123!");

                if (result.Succeeded)
                {
                    logger.LogInformation("Admin creado OK");
                    var roleResult = await userManager.AddToRoleAsync(user, "Administrador");
                    if (roleResult.Succeeded)
                        logger.LogInformation("Rol Administrador asignado OK");
                    else
                        logger.LogError($"Error al asignar rol: {string.Join(", ", roleResult.Errors)}");
                }
                else
                {
                    foreach (var err in result.Errors)
                        logger.LogError($"Error al crear admin: {err.Description}");
                }
            }
            else
            {
                logger.LogInformation("Admin ya existe");

                // Asegurar que tenga el rol
                if (!await userManager.IsInRoleAsync(user, "Administrador"))
                {
                    await userManager.AddToRoleAsync(user, "Administrador");
                    logger.LogInformation("Rol Administrador reasignado");
                }
            }

            // =============================================
            // 3. DATOS INICIALES
            // =============================================
            try
            {
                if (!context.Horarios.Any())
                {
                    context.Horarios.AddRange(
                        new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0) },
                        new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0) },
                        new Horario { Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0) },
                        new Horario { Dia = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0) },
                        new Horario { Dia = DayOfWeek.Thursday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0) },
                        new Horario { Dia = DayOfWeek.Friday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0) }
                    );
                    logger.LogInformation("Horarios creados");
                }

                if (!context.Aulas.Any())
                {
                    context.Aulas.AddRange(
                        new Aula { Codigo = "A101", Nombre = "Aula 101", Capacidad = 30 },
                        new Aula { Codigo = "A102", Nombre = "Aula 102", Capacidad = 30 },
                        new Aula { Codigo = "LAB1", Nombre = "Laboratorio 1", Capacidad = 25, EsLaboratorio = true }
                    );
                    logger.LogInformation("Aulas creadas");
                }

                if (!context.ConfiguracionesInstitucionales.Any())
                {
                    context.ConfiguracionesInstitucionales.Add(new ConfiguracionInstitucional
                    {
                        NombreInstitucion = "Instituto Superior",
                        PagoInicialInscripcion = 500,
                        CostoBaseMateria = 300,
                        PorcentajePenalizacionMora = 5,
                        DiaInicioPagos = 23,
                        DiaFinPagos = 30,
                        PeriodoActual = "2026-I"
                    });
                    logger.LogInformation("Configuracion creada");
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Datos iniciales guardados OK");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error al insertar datos iniciales: {ex.Message}");
            }

            logger.LogInformation("=== DATA INITIALIZER COMPLETADO ===");
        }
    }
}