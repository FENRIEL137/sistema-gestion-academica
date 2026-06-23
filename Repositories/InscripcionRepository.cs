using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class InscripcionRepository : GenericRepository<Inscripcion>, IInscripcionRepository
    {
        public InscripcionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Inscripcion>> GetInscripcionesPorEstudianteAsync(int estudianteId)
        {
            return await _dbSet
                .Include(i => i.Materia)
                .ThenInclude(m => m.Profesor)
                .Include(i => i.Materia.Aula)
                .Include(i => i.Materia.Horario)
                .Where(i => i.EstudianteId == estudianteId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscripcion>> GetInscripcionesPorMateriaAsync(int materiaId)
        {
            return await _dbSet
                .Include(i => i.Estudiante)
                .Include(i => i.Notas)
                .Where(i => i.MateriaId == materiaId && i.Estado == EstadoInscripcion.Activa)
                .ToListAsync();
        }

        public async Task<Inscripcion?> GetInscripcionConDetallesAsync(int inscripcionId)
        {
            return await _dbSet
                .Include(i => i.Estudiante)
                .Include(i => i.Materia)
                .ThenInclude(m => m.Profesor)
                .Include(i => i.Materia.Actividades)
                .Include(i => i.Notas)
                .ThenInclude(n => n.Actividad)
                .FirstOrDefaultAsync(i => i.Id == inscripcionId);
        }

        public async Task<bool> EstaInscritoAsync(int estudianteId, int materiaId)
        {
            return await _dbSet.AnyAsync(i =>
                i.EstudianteId == estudianteId &&
                i.MateriaId == materiaId &&
                i.Estado == EstadoInscripcion.Activa);
        }

        public async Task<decimal> CalcularNotaFinalAsync(int inscripcionId)
        {
            var notas = await _context.Notas
                .Include(n => n.Actividad)
                .Where(n => n.InscripcionId == inscripcionId)
                .ToListAsync();

            if (!notas.Any()) return 0;

            decimal notaFinal = 0;
            foreach (var nota in notas)
            {
                notaFinal += (nota.Calificacion * nota.Actividad.ValorPorcentual) / 100;
            }

            // Actualizar nota final en la inscripción
            var inscripcion = await _dbSet.FindAsync(inscripcionId);
            if (inscripcion != null)
            {
                inscripcion.NotaFinal = Math.Round(notaFinal, 2);
                await _context.SaveChangesAsync();
            }

            return Math.Round(notaFinal, 2);
        }

        public async Task<IEnumerable<Inscripcion>> GetInscripcionesPendientesPagoAsync()
        {
            return await _dbSet
                .Include(i => i.Estudiante)
                .Include(i => i.Materia)
                .Where(i => !i.PagoRealizado && i.Estado == EstadoInscripcion.Activa)
                .ToListAsync();
        }
    }
}