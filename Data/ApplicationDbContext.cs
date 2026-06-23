using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<ConfiguracionInstitucional> ConfiguracionesInstitucionales { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración de ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(200);
            });

            // Configuración de índices únicos
            builder.Entity<Estudiante>().HasIndex(e => e.CI).IsUnique();
            builder.Entity<Profesor>().HasIndex(p => p.CI).IsUnique();
            builder.Entity<Empleado>().HasIndex(e => e.CI).IsUnique();

            // Configuración de relaciones
            builder.Entity<Inscripcion>()
                .HasOne(i => i.Estudiante)
                .WithMany(e => e.Inscripciones)
                .HasForeignKey(i => i.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inscripcion>()
                .HasOne(i => i.Materia)
                .WithMany(m => m.Inscripciones)
                .HasForeignKey(i => i.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Nota>()
                .HasOne(n => n.Inscripcion)
                .WithMany(i => i.Notas)
                .HasForeignKey(n => n.InscripcionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Nota>()
                .HasOne(n => n.Actividad)
                .WithMany(a => a.Notas)
                .HasForeignKey(n => n.ActividadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Actividad>()
                .HasOne(a => a.Materia)
                .WithMany(m => m.Actividades)
                .HasForeignKey(a => a.MateriaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Materia>()
                .HasOne(m => m.Profesor)
                .WithMany(p => p.Materias)
                .HasForeignKey(m => m.ProfesorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Materia>()
                .HasOne(m => m.Aula)
                .WithMany(a => a.Materias)
                .HasForeignKey(m => m.AulaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Materia>()
                .HasOne(m => m.Horario)
                .WithMany(h => h.Materias)
                .HasForeignKey(m => m.HorarioId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Pago>()
                .HasOne(p => p.Inscripcion)
                .WithMany(i => i.Pagos)
                .HasForeignKey(p => p.InscripcionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Pago>()
                .HasOne(p => p.Estudiante)
                .WithMany()
                .HasForeignKey(p => p.EstudianteId)
                .OnDelete(DeleteBehavior.SetNull);

            // =============================================
            // DATOS INICIALES (Seed Data) - VALORES FIJOS
            // =============================================

            // Aulas iniciales
            builder.Entity<Aula>().HasData(
                new Aula { Id = 1, Codigo = "A101", Nombre = "Aula 101", Capacidad = 30, EsLaboratorio = false, Ubicacion = "Primer Piso", Activo = true },
                new Aula { Id = 2, Codigo = "A102", Nombre = "Aula 102", Capacidad = 30, EsLaboratorio = false, Ubicacion = "Primer Piso", Activo = true },
                new Aula { Id = 3, Codigo = "A201", Nombre = "Aula 201", Capacidad = 35, EsLaboratorio = false, Ubicacion = "Segundo Piso", Activo = true },
                new Aula { Id = 4, Codigo = "LAB1", Nombre = "Laboratorio de Computacion", Capacidad = 25, EsLaboratorio = true, Ubicacion = "Primer Piso", Activo = true },
                new Aula { Id = 5, Codigo = "LAB2", Nombre = "Laboratorio de Ciencias", Capacidad = 20, EsLaboratorio = true, Ubicacion = "Segundo Piso", Activo = true }
            );

            // Horarios iniciales
            builder.Entity<Horario>().HasData(
                new Horario { Id = 1, Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                new Horario { Id = 2, Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                new Horario { Id = 3, Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true },
                new Horario { Id = 4, Dia = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Activo = true },
                new Horario { Id = 5, Dia = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Activo = true },
                new Horario { Id = 6, Dia = DayOfWeek.Thursday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(10, 0, 0), Activo = true }
            );

            // Configuración institucional inicial
            builder.Entity<ConfiguracionInstitucional>().HasData(
                new ConfiguracionInstitucional
                {
                    Id = 1,
                    NombreInstitucion = "Instituto de Educacion Superior",
                    PagoInicialInscripcion = 500.00m,
                    CostoBaseMateria = 300.00m,
                    PorcentajePenalizacionMora = 5.00m,
                    DiaInicioPagos = 23,
                    DiaFinPagos = 30,
                    PeriodoActual = "2026-I",
                    FechaUltimaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                    Activo = true
                }
            );
        }
    }
}