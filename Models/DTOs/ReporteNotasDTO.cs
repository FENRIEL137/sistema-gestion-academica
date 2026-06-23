namespace SistemaGestionAcademica.Models.DTOs
{
    /// <summary>
    /// DTO para exportación de reporte de notas a Excel
    /// </summary>
    public class ReporteNotasDTO
    {
        public string MateriaNombre { get; set; } = string.Empty;
        public string ProfesorNombre { get; set; } = string.Empty;
        public List<EstudianteNotaDTO> Estudiantes { get; set; } = new();
        public List<string> ActividadesNombres { get; set; } = new();
    }

    public class EstudianteNotaDTO
    {
        public string CI { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public Dictionary<string, decimal?> NotasPorActividad { get; set; } = new();
        public decimal? NotaFinal { get; set; }
    }
}