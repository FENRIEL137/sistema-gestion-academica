using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class EstudianteRepository : GenericRepository<Estudiante>, IEstudianteRepository
    {
        public EstudianteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Estudiante?> GetByCIAsync(string ci)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.CI == ci);
        }

        public async Task<Estudiante?> GetByUserIdAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<IEnumerable<Estudiante>> GetEstudiantesActivosAsync()
        {
            return await _dbSet.Where(e => e.Activo).ToListAsync();
        }

        public async Task<IEnumerable<Estudiante>> GetEstudiantesConDeudasAsync()
        {
            return await _dbSet
                .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Materia)
                .Where(e => e.Activo && e.Inscripciones.Any(i => !i.PagoRealizado && i.Estado == EstadoInscripcion.Activa))
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscripcion>> GetInscripcionesEstudianteAsync(int estudianteId)
        {
            return await _context.Inscripciones
                .Include(i => i.Materia)
                .ThenInclude(m => m.Profesor)
                .Include(i => i.Materia.Aula)
                .Include(i => i.Materia.Horario)
                .Where(i => i.EstudianteId == estudianteId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetHistorialPagosAsync(int estudianteId)
        {
            return await _context.Pagos
                .Where(p => p.EstudianteId == estudianteId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<decimal> GetDeudaTotalAsync(int estudianteId)
        {
            var inscripciones = await _context.Inscripciones
                .Include(i => i.Materia)
                .Where(i => i.EstudianteId == estudianteId &&
                           !i.PagoRealizado &&
                           i.Estado == EstadoInscripcion.Activa)
                .ToListAsync();

            return inscripciones.Sum(i => i.Materia.Costo);
        }

        public async Task<bool> TienePagoInicialAsync(int estudianteId)
        {
            var estudiante = await GetByIdAsync(estudianteId);
            return estudiante != null && estudiante.PagoInicial > 0;
        }
    }
}