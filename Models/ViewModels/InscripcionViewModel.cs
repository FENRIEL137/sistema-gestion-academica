using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAcademica.Models.ViewModels
{
    public class InscripcionViewModel
    {
        public int EstudianteId { get; set; }
        public List<MateriaDisponibleViewModel> MateriasDisponibles { get; set; } = new();
        public List<MateriaInscritaViewModel> MateriasInscritas { get; set; } = new();
    }

    public class MateriaDisponibleViewModel
    {
        public int MateriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Costo { get; set; }
        public string Profesor { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
        public bool EstaInscrito { get; set; }
    }

    public class MateriaInscritaViewModel
    {
        public int InscripcionId { get; set; }
        public int MateriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Costo { get; set; }
        public bool PagoRealizado { get; set; }
        public decimal? NotaFinal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}