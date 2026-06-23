using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAcademica.Models.ViewModels
{
    public class PagoViewModel
    {
        public int EstudianteId { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public decimal DeudaTotal { get; set; }
        public List<PagoPendienteViewModel> PagosPendientes { get; set; } = new();
        public List<PagoRealizadoViewModel> HistorialPagos { get; set; } = new();
    }

    public class PagoPendienteViewModel
    {
        public int InscripcionId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaInscripcion { get; set; }
    }

    public class PagoRealizadoViewModel
    {
        public int PagoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}