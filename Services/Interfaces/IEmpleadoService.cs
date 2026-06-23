using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Services.Interfaces
{
    public interface IEmpleadoService
    {
        Task<Estudiante> RegistrarEstudianteAsync(Estudiante estudiante, string email, string password);
        Task<Estudiante> ActualizarEstudianteAsync(Estudiante estudiante);
        Task<bool> DarBajaEstudianteAsync(int id);
        Task<bool> ReactivarEstudianteAsync(int id);
        Task<Pago> RegistrarPagoAsync(int inscripcionId, decimal monto, string concepto, string usuarioId);
        Task<Inscripcion> RegistrarInscripcionAsync(int estudianteId, int materiaId);
        Task<bool> DarBajaInscripcionAsync(int id);
        Task<bool> EsPeriodoPagoAsync();
    }
}