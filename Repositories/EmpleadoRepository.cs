using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class EmpleadoRepository : GenericRepository<Empleado>, IEmpleadoRepository
    {
        public EmpleadoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Empleado?> GetByCIAsync(string ci)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.CI == ci);
        }

        public async Task<Empleado?> GetByUserIdAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<IEnumerable<Empleado>> GetEmpleadosActivosAsync()
        {
            return await _dbSet.Where(e => e.Activo).ToListAsync();
        }
    }
}