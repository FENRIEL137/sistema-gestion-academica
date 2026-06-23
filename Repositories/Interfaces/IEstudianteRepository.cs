using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IEstudianteRepository : IGenericRepository<Estudiante>
    {
        // Métodos específicos para Estudiante
        Task<Estudiante?> GetByCIAsync(string ci);
        Task<Estudiante?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Estudiante>> GetEstudiantesActivosAsync();
        Task<IEnumerable<Estudiante>> GetEstudiantesConDeudasAsync();
        Task<IEnumerable<Inscripcion>> GetInscripcionesEstudianteAsync(int estudianteId);
        Task<IEnumerable<Pago>> GetHistorialPagosAsync(int estudianteId);
        Task<decimal> GetDeudaTotalAsync(int estudianteId);
        Task<bool> TienePagoInicialAsync(int estudianteId);
    }
}