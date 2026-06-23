using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Nota
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Inscripción")]
        public int InscripcionId { get; set; }

        [ForeignKey("InscripcionId")]
        public virtual Inscripcion Inscripcion { get; set; } = null!;

        [Required]
        [Display(Name = "Actividad")]
        public int ActividadId { get; set; }

        [ForeignKey("ActividadId")]
        public virtual Actividad Actividad { get; set; } = null!;

        [Required]
        [Range(0, 100, ErrorMessage = "La calificación debe estar entre 0 y 100")]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Calificación")]
        public decimal Calificacion { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Display(Name = "Modificado Por")]
        public string? ModificadoPorId { get; set; }

        [ForeignKey("ModificadoPorId")]
        public virtual ApplicationUser? ModificadoPor { get; set; }

        [Display(Name = "Última Modificación")]
        public DateTime? UltimaModificacion { get; set; }
    }
}