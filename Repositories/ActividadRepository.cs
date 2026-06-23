using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class ActividadRepository : GenericRepository<Actividad>, IActividadRepository
    {
        public ActividadRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Actividad>> GetActividadesPorMateriaAsync(int materiaId)
        {
            return await _dbSet
                .Where(a => a.MateriaId == materiaId && a.Activo)
                .OrderBy(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<decimal> GetSumaPorcentajesAsync(int materiaId)
        {
            return await _dbSet
                .Where(a => a.MateriaId == materiaId && a.Activo)
                .SumAsync(a => a.ValorPorcentual);
        }

        public async Task<bool> ValidarPorcentajesAsync(int materiaId, decimal nuevoPorcentaje, int? actividadId = null)
        {
            var sumaActual = await _dbSet
                .Where(a => a.MateriaId == materiaId && a.Activo && a.Id != actividadId)
                .SumAsync(a => a.ValorPorcentual);

            return (sumaActual + nuevoPorcentaje) <= 100;
        }

        public async Task<IEnumerable<Actividad>> GetActividadesPorTipoAsync(int materiaId, TipoActividad tipo)
        {
            return await _dbSet
                .Where(a => a.MateriaId == materiaId && a.Tipo == tipo && a.Activo)
                .ToListAsync();
        }
    }
}