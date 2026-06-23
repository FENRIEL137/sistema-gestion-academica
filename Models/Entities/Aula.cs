using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Aula
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código del aula es requerido")]
        [StringLength(20)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del aula es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre del Aula")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La capacidad es requerida")]
        [Range(1, 200, ErrorMessage = "La capacidad debe estar entre 1 y 200")]
        [Display(Name = "Capacidad")]
        public int Capacidad { get; set; }

        [Display(Name = "¿Es Laboratorio?")]
        public bool EsLaboratorio { get; set; } = false;

        [StringLength(200)]
        [Display(Name = "Ubicación")]
        public string? Ubicacion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<Materia> Materias { get; set; } = new List<Materia>();
    }
}