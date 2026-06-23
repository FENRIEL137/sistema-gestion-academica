using SistemaGestionAcademica.Models.DTOs;

namespace SistemaGestionAcademica.Services.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de exportación a Excel
    /// </summary>
    public interface IExcelExportService
    {
        Task<byte[]> ExportarNotasMateriaAsync(int materiaId);
        Task<byte[]> ExportarListadoEstudiantesAsync();
        Task<byte[]> ExportarListadoProfesoresAsync();
        Task<byte[]> ExportarReportePagosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<byte[]> ExportarEstudiantesConDeudaAsync();
        Task<byte[]> ExportarEstadoAcademicoEstudianteAsync(int estudianteId);
    }
}