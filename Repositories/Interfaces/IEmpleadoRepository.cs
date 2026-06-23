using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IEmpleadoRepository : IGenericRepository<Empleado>
    {
        Task<Empleado?> GetByCIAsync(string ci);
        Task<Empleado?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Empleado>> GetEmpleadosActivosAsync();
    }
}