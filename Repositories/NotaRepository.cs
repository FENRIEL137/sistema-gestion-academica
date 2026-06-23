using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class NotaRepository : GenericRepository<Nota>, INotaRepository
    {
        public NotaRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene todas las notas de una inscripción específica
        /// </summary>
        public async Task<IEnumerable<Nota>> GetNotasPorInscripcionAsync(int inscripcionId)
        {
            return await _dbSet
                .Include(n => n.Actividad)
                .Where(n => n.InscripcionId == inscripcionId)
                .OrderBy(n => n.Actividad.Fecha)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las notas de una actividad específica
        /// </summary>
        public async Task<IEnumerable<Nota>> GetNotasPorActividadAsync(int actividadId)
        {
            return await _dbSet
                .Include(n => n.Inscripcion)
                .ThenInclude(i => i.Estudiante)
                .Where(n => n.ActividadId == actividadId)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una nota específica por inscripción y actividad
        /// </summary>
        public async Task<Nota?> GetNotaAsync(int inscripcionId, int actividadId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(n => n.InscripcionId == inscripcionId
                                        && n.ActividadId == actividadId);
        }

        /// <summary>
        /// Obtiene un diccionario con las notas agrupadas por actividad
        /// Key: ActividadId, Value: Calificacion
        /// </summary>
        public async Task<Dictionary<int, decimal>> GetDiccionarioNotasPorActividadAsync(int inscripcionId)
        {
            var notas = await _dbSet
                .Where(n => n.InscripcionId == inscripcionId)
                .ToListAsync();

            return notas.ToDictionary(n => n.ActividadId, n => n.Calificacion);
        }
    }
}