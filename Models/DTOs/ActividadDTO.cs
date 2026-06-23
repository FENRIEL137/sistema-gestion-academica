namespace SistemaGestionAcademica.Models.DTOs
{
    public class ActividadDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal ValorPorcentual { get; set; }
    }
}