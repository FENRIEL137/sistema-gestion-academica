using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Services.Interfaces
{
    public interface IPagoService
    {
        Task<Pago> ProcesarPagoAsync(int estudianteId, int inscripcionId, decimal monto, string concepto);
        Task<IEnumerable<Pago>> GetHistorialPagosAsync(int estudianteId);
        Task<decimal> GetDeudaTotalAsync(int estudianteId);
        Task<bool> VerificarPagoInicialAsync(int estudianteId);
        Task<bool> EsPeriodoPagoAsync();
        Task AplicarPenalizacionMoraAsync();
    }
}