namespace SistemaGestionAcademica.Models.DTOs
{
    public class PagoDTO
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}