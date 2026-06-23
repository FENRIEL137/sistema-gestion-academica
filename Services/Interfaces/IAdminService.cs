using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Services.Interfaces
{
    public interface IAdminService
    {
        // Gestión de Profesores
        Task<Profesor> CrearProfesorAsync(Profesor profesor, string email, string password);
        Task<Profesor> ActualizarProfesorAsync(Profesor profesor);
        Task<bool> DarBajaProfesorAsync(int id);
        Task<bool> ContratarProfesorAsync(int id);

        // Gestión de Empleados
        Task<Empleado> CrearEmpleadoAsync(Empleado empleado, string email, string password);
        Task<Empleado> ActualizarEmpleadoAsync(Empleado empleado);
        Task<bool> DarBajaEmpleadoAsync(int id);

        // Gestión de Materias
        Task<Materia> CrearMateriaAsync(Materia materia);
        Task<Materia> ActualizarMateriaAsync(Materia materia);

        // Gestión de Aulas
        Task<Aula> CrearAulaAsync(Aula aula);
        Task<Aula> ActualizarAulaAsync(Aula aula);

        // Configuración
        Task<ConfiguracionInstitucional> GetConfiguracionAsync();
        Task ActualizarConfiguracionAsync(ConfiguracionInstitucional config, string usuarioId);

        // Reportes
        Task<byte[]> ExportarListadoEstudiantesAsync();
        Task<byte[]> ExportarListadoProfesoresAsync();
        Task<byte[]> ExportarReportePagosAsync(DateTime? inicio, DateTime? fin);
        Task<byte[]> ExportarEstudiantesConDeudaAsync();
    }
}