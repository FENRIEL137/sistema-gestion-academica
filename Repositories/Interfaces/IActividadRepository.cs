using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IActividadRepository : IGenericRepository<Actividad>
    {
        Task<IEnumerable<Actividad>> GetActividadesPorMateriaAsync(int materiaId);
        Task<decimal> GetSumaPorcentajesAsync(int materiaId);
        Task<bool> ValidarPorcentajesAsync(int materiaId, decimal nuevoPorcentaje, int? actividadId = null);
        Task<IEnumerable<Actividad>> GetActividadesPorTipoAsync(int materiaId, TipoActividad tipo);
    }
}