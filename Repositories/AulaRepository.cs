using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class AulaRepository : GenericRepository<Aula>, IAulaRepository
    {
        public AulaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Aula>> GetAulasDisponiblesAsync()
        {
            return await _dbSet
                .Where(a => a.Activo)
                .Include(a => a.Materias)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aula>> GetLaboratoriosAsync()
        {
            return await _dbSet
                .Where(a => a.EsLaboratorio && a.Activo)
                .ToListAsync();
        }

        public async Task<bool> EstaAulaDisponibleAsync(int aulaId, int horarioId)
        {
            var materiaEnAulaYHorario = await _context.Materias
                .AnyAsync(m => m.AulaId == aulaId && m.HorarioId == horarioId && m.Activo);

            return !materiaEnAulaYHorario;
        }
    }
}