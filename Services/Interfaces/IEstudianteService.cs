using SistemaGestionAcademica.Models.DTOs;
using SistemaGestionAcademica.Models.ViewModels;

namespace SistemaGestionAcademica.Services.Interfaces
{
    public interface IEstudianteService
    {
        // Inscripciones
        Task<IEnumerable<MateriaDTO>> GetMateriasDisponiblesAsync();
        Task<bool> InscribirMateriaAsync(int estudianteId, int materiaId);
        Task<IEnumerable<InscripcionDTO>> GetMateriasInscritasAsync(int estudianteId);
        Task<HorarioCompletoDTO> GetHorarioEstudianteAsync(int estudianteId);

        // Pagos
        Task<bool> RealizarPagoAsync(int estudianteId, int inscripcionId, decimal monto);
        Task<IEnumerable<PagoDTO>> GetHistorialPagosAsync(int estudianteId);
        Task<decimal> GetDeudaTotalAsync(int estudianteId);
        Task<IEnumerable<DeudaDTO>> GetDeudasPendientesAsync(int estudianteId);

        // Consultas
        Task<IEnumerable<NotaDTO>> GetNotasAsync(int estudianteId);
        Task<IEnumerable<ActividadDTO>> GetActividadesAsync(int estudianteId, int materiaId);
        Task<EstadoAcademicoDTO> GetEstadoAcademicoAsync(int estudianteId);
    }
}