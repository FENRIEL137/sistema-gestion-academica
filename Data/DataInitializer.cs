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
                    await roleManager.CreateAsync(new ApplicationRole(roleName));
                    logger.LogInformation($"Rol creado: {roleName}");
                }
            }

            // Crear admin
            var adminEmail = "admin@sistema.edu";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                    logger.LogInformation("Admin creado exitosamente.");
                }
            }

            // =============================================
            // INSERTAR HORARIOS SI NO EXISTEN
            // =============================================
            if (!context.Horarios.Any())
            {
                logger.LogInformation("Creando horarios iniciales...");

                context.Horarios.AddRange(
                    new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Thursday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Thursday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Friday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Friday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true }
                );

                await context.SaveChangesAsync();
                logger.LogInformation("Horarios creados exitosamente.");
            }
            else
            {
                logger.LogInformation($"Ya existen {context.Horarios.Count()} horarios.");
            }

            // Insertar aulas si no existen
            if (!context.Aulas.Any())
            {
                context.Aulas.AddRange(
                    new Aula { Codigo = "A101", Nombre = "Aula 101", Capacidad = 30 },
                    new Aula { Codigo = "A102", Nombre = "Aula 102", Capacidad = 30 },
                    new Aula { Codigo = "A201", Nombre = "Aula 201", Capacidad = 35 },
                    new Aula { Codigo = "LAB1", Nombre = "Laboratorio 1", Capacidad = 25, EsLaboratorio = true }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Aulas creadas exitosamente.");
            }

            // Insertar configuracion si no existe
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
                        FechaUltimaActualizacion = DateTime.UtcNow,
                        Activo = true
                    }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Configuracion creada exitosamente.");
            }

            logger.LogInformation("=== DATA INITIALIZER COMPLETADO ===");
        }
    }
}