using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IConfiguracionInstitucionalRepository : IGenericRepository<ConfiguracionInstitucional>
    {
        Task<ConfiguracionInstitucional?> GetConfiguracionActualAsync();
        Task ActualizarConfiguracionAsync(ConfiguracionInstitucional configuracion, string usuarioId);
    }
}