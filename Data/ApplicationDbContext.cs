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

            // Configuracion de ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(200);
            });

            // Configuracion de indices unicos
            builder.Entity<Estudiante>().HasIndex(e => e.CI).IsUnique();
            builder.Entity<Profesor>().HasIndex(p => p.CI).IsUnique();
            builder.Entity<Empleado>().HasIndex(e => e.CI).IsUnique();

            // Configuracion de relaciones
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
            // DATOS INICIALES (Seed Data) - CORREGIDO
            // =============================================
            // NOTA: NO usamos IDs fijos, dejamos que la BD los genere
            // Los datos iniciales se insertan en DataInitializer.cs
        }
    }
}