using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Materia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la materia es requerido")]
        [StringLength(150)]
        [Display(Name = "Nombre de la Materia")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El costo es requerido")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Costo")]
        public decimal Costo { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relaciones
        [Display(Name = "Profesor")]
        public int? ProfesorId { get; set; }

        [ForeignKey("ProfesorId")]
        public virtual Profesor? Profesor { get; set; }

        [Display(Name = "Aula")]
        public int? AulaId { get; set; }

        [ForeignKey("AulaId")]
        public virtual Aula? Aula { get; set; }

        [Display(Name = "Horario")]
        public int? HorarioId { get; set; }

        [ForeignKey("HorarioId")]
        public virtual Horario? Horario { get; set; }

        // Relaciones
        public virtual ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public virtual ICollection<Actividad> Actividades { get; set; } = new List<Actividad>();
    }
}