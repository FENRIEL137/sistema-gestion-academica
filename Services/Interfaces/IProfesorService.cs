using SistemaGestionAcademica.Models.DTOs;
using SistemaGestionAcademica.Models.ViewModels;

namespace SistemaGestionAcademica.Services.Interfaces
{
    public interface IProfesorService
    {
        // Gestión de Materias
        Task<IEnumerable<MateriaDTO>> GetMateriasAsignadasAsync(int profesorId);
        Task<IEnumerable<EstudianteDTO>> GetEstudiantesInscritosAsync(int materiaId);

        // Gestión de Actividades
        Task<IEnumerable<ActividadDTO>> GetActividadesAsync(int materiaId);
        Task<ActividadDTO> CrearActividadAsync(ActividadViewModel model);
        Task<ActividadDTO> ActualizarActividadAsync(ActividadViewModel model);
        Task<bool> EliminarActividadAsync(int actividadId);

        // Gestión de Notas
        Task<NotaViewModel> GetVistaNotasAsync(int materiaId);
        Task<bool> RegistrarNotaAsync(RegistrarNotaViewModel model, string profesorId);
        Task<bool> ActualizarNotaAsync(int notaId, decimal calificacion, string profesorId);
        Task<bool> EliminarNotaAsync(int notaId);
        Task<decimal> CalcularNotaFinalAsync(int inscripcionId);

        // Reportes
        Task<ReporteNotasDTO> GenerarReporteNotasAsync(int materiaId);
        Task<byte[]> ExportarExcelAsync(int materiaId);
    }
}