using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAcademica.Models.ViewModels
{
    public class NotaViewModel
    {
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public List<EstudianteNotaViewModel> Estudiantes { get; set; } = new();
        public List<string> ActividadesNombres { get; set; } = new();
    }

    public class EstudianteNotaViewModel
    {
        public int InscripcionId { get; set; }
        public int EstudianteId { get; set; }
        public string EstudianteCI { get; set; } = string.Empty;
        public string EstudianteNombre { get; set; } = string.Empty;
        public Dictionary<int, decimal?> Notas { get; set; } = new();
        public decimal? NotaFinal { get; set; }
    }

    public class RegistrarNotaViewModel
    {
        [Required]
        public int InscripcionId { get; set; }

        [Required]
        public int ActividadId { get; set; }

        [Required(ErrorMessage = "La calificación es requerida")]
        [Range(0, 100, ErrorMessage = "La calificación debe estar entre 0 y 100")]
        [Display(Name = "Calificación")]
        public decimal Calificacion { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
    }
}