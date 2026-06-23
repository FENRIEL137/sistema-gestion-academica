using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IInscripcionRepository : IGenericRepository<Inscripcion>
    {
        Task<IEnumerable<Inscripcion>> GetInscripcionesPorEstudianteAsync(int estudianteId);
        Task<IEnumerable<Inscripcion>> GetInscripcionesPorMateriaAsync(int materiaId);
        Task<Inscripcion?> GetInscripcionConDetallesAsync(int inscripcionId);
        Task<bool> EstaInscritoAsync(int estudianteId, int materiaId);
        Task<decimal> CalcularNotaFinalAsync(int inscripcionId);
        Task<IEnumerable<Inscripcion>> GetInscripcionesPendientesPagoAsync();
    }
}