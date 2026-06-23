using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class HorarioRepository : GenericRepository<Horario>, IHorarioRepository
    {
        public HorarioRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Horario>> GetHorariosDisponiblesAsync()
        {
            return await _dbSet
                .Where(h => h.Activo)
                .ToListAsync();
        }
    }
}