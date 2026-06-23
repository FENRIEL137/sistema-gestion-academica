using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Data
{
    public static class DataInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // =============================================
            // 1. CREAR ROLES
            // =============================================
            string[] roles = { "Administrador", "Empleado", "Profesor", "Estudiante" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole(role)
                    {
                        Descripcion = $"Rol de {role} del sistema"
                    });
                }
            }

            // =============================================
            // 2. CREAR USUARIO ADMINISTRADOR
            // =============================================
            var adminEmail = "admin@sistema.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                }
            }

            // =============================================
            // 3. INSERTAR AULAS INICIALES (si no existen)
            // =============================================
            if (!context.Aulas.Any())
            {
                context.Aulas.AddRange(
                    new Aula { Codigo = "A101", Nombre = "Aula 101", Capacidad = 30, EsLaboratorio = false, Ubicacion = "Primer Piso", Activo = true },
                    new Aula { Codigo = "A102", Nombre = "Aula 102", Capacidad = 30, EsLaboratorio = false, Ubicacion = "Primer Piso", Activo = true },
                    new Aula { Codigo = "A201", Nombre = "Aula 201", Capacidad = 35, EsLaboratorio = false, Ubicacion = "Segundo Piso", Activo = true },
                    new Aula { Codigo = "LAB1", Nombre = "Laboratorio de Computacion", Capacidad = 25, EsLaboratorio = true, Ubicacion = "Primer Piso", Activo = true },
                    new Aula { Codigo = "LAB2", Nombre = "Laboratorio de Ciencias", Capacidad = 20, EsLaboratorio = true, Ubicacion = "Segundo Piso", Activo = true }
                );
            }

            // =============================================
            // 4. INSERTAR HORARIOS INICIALES (si no existen)
            // =============================================
            if (!context.Horarios.Any())
            {
                context.Horarios.AddRange(
                    new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Activo = true },
                    new Horario { Dia = DayOfWeek.Thursday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true }
                );
            }

            // =============================================
            // 5. INSERTAR CONFIGURACION INICIAL (si no existe)
            // =============================================
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
            }

            await context.SaveChangesAsync();
        }
    }
}