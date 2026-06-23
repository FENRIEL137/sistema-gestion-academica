namespace SistemaGestionAcademica.Models.DTOs
{
    public class NotaDTO
    {
        public int Id { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string ActividadNombre { get; set; } = string.Empty;
        public string TipoActividad { get; set; } = string.Empty;
        public decimal Calificacion { get; set; }
        public decimal ValorPorcentual { get; set; }
        public decimal Ponderacion => (Calificacion * ValorPorcentual) / 100;
    }
}