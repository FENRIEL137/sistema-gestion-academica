using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class PagoRepository : GenericRepository<Pago>, IPagoRepository
    {
        public PagoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pago>> GetPagosPorEstudianteAsync(int estudianteId)
        {
            return await _dbSet
                .Include(p => p.Inscripcion)
                .ThenInclude(i => i.Materia)
                .Where(p => p.EstudianteId == estudianteId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetPagosPorInscripcionAsync(int inscripcionId)
        {
            return await _dbSet
                .Where(p => p.InscripcionId == inscripcionId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPagadoAsync(int estudianteId)
        {
            return await _dbSet
                .Where(p => p.EstudianteId == estudianteId && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        public async Task<decimal> GetDeudaPendienteAsync(int estudianteId)
        {
            var inscripciones = await _context.Inscripciones
                .Include(i => i.Materia)
                .Where(i => i.EstudianteId == estudianteId &&
                           !i.PagoRealizado &&
                           i.Estado == EstadoInscripcion.Activa)
                .ToListAsync();

            return inscripciones.Sum(i => i.Materia.Costo);
        }

        public async Task<IEnumerable<Pago>> GetPagosPendientesAsync()
        {
            return await _dbSet
                .Include(p => p.Estudiante)
                .Where(p => p.Estado == EstadoPago.Pendiente)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetPagosPorPeriodoAsync(DateTime inicio, DateTime fin)
        {
            return await _dbSet
                .Include(p => p.Estudiante)
                .Include(p => p.Inscripcion)
                .Where(p => p.FechaPago >= inicio && p.FechaPago <= fin)
                .OrderBy(p => p.FechaPago)
                .ToListAsync();
        }
    }
}