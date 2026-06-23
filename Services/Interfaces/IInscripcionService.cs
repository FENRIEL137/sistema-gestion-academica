using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Services.Interfaces
{
    public interface IInscripcionService
    {
        Task<Inscripcion> InscribirAsync(int estudianteId, int materiaId);
        Task<bool> CancelarInscripcionAsync(int inscripcionId);
        Task<bool> ReactivarInscripcionAsync(int inscripcionId);
        Task<IEnumerable<Inscripcion>> GetInscripcionesActivasAsync(int estudianteId);
        Task<bool> VerificarDisponibilidadAsync(int materiaId);
    }
}