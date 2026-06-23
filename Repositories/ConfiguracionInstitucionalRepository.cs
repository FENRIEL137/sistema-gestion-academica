using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class ConfiguracionInstitucionalRepository : GenericRepository<ConfiguracionInstitucional>, IConfiguracionInstitucionalRepository
    {
        public ConfiguracionInstitucionalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ConfiguracionInstitucional?> GetConfiguracionActualAsync()
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Activo);
        }

        public async Task ActualizarConfiguracionAsync(ConfiguracionInstitucional configuracion, string usuarioId)
        {
            configuracion.ActualizadoPorId = usuarioId;
            configuracion.FechaUltimaActualizacion = DateTime.Now;
            _dbSet.Update(configuracion);
            await _context.SaveChangesAsync();
        }
    }
}