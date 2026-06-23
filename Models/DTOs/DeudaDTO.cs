namespace SistemaGestionAcademica.Models.DTOs
{
    public class DeudaDTO
    {
        public int InscripcionId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public int DiasAtraso => (DateTime.Now - FechaInscripcion).Days;
    }
}