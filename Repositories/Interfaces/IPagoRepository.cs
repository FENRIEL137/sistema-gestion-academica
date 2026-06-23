using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IPagoRepository : IGenericRepository<Pago>
    {
        Task<IEnumerable<Pago>> GetPagosPorEstudianteAsync(int estudianteId);
        Task<IEnumerable<Pago>> GetPagosPorInscripcionAsync(int inscripcionId);
        Task<decimal> GetTotalPagadoAsync(int estudianteId);
        Task<decimal> GetDeudaPendienteAsync(int estudianteId);
        Task<IEnumerable<Pago>> GetPagosPendientesAsync();
        Task<IEnumerable<Pago>> GetPagosPorPeriodoAsync(DateTime inicio, DateTime fin);
    }
}