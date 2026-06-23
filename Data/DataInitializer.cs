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

            // Roles
            string[] roles = { "Administrador", "Empleado", "Profesor", "Estudiante" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                {
                    await roleManager.CreateAsync(new ApplicationRole(r));
                    logger.LogInformation("Rol creado: " + r);
                }
            }

            // Admin
           

            // Horarios
            if (!context.Horarios.Any())
            {
                context.Horarios.AddRange(
                    new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0) },
                    new Horario { Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0) }
                );
            }

            // Aulas
            if (!context.Aulas.Any())
            {
                context.Aulas.AddRange(
                    new Aula { Codigo = "A101", Nombre = "Aula 101", Capacidad = 30 },
                    new Aula { Codigo = "LAB1", Nombre = "Laboratorio 1", Capacidad = 25, EsLaboratorio = true }
                );
            }

            // Config
            if (!context.ConfiguracionesInstitucionales.Any())
            {
                context.ConfiguracionesInstitucionales.Add(new ConfiguracionInstitucional
                {
                    NombreInstitucion = "Instituto Superior",
                    PagoInicialInscripcion = 500,
                    CostoBaseMateria = 300,
                    DiaInicioPagos = 23,
                    DiaFinPagos = 30,
                    PeriodoActual = "2026-I"
                });
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Datos iniciales OK");
        }
    }
}