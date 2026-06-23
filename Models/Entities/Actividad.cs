using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Actividad
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la actividad es requerido")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Tipo de Actividad")]
        public TipoActividad Tipo { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "El valor porcentual debe estar entre 0 y 100")]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Valor Porcentual")]
        public decimal ValorPorcentual { get; set; }

        [Required]
        [Display(Name = "Materia")]
        public int MateriaId { get; set; }

        [ForeignKey("MateriaId")]
        public virtual Materia Materia { get; set; } = null!;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<Nota> Notas { get; set; } = new List<Nota>();
    }

    public enum TipoActividad
    {
        [Display(Name = "Tarea")]
        Tarea = 1,

        [Display(Name = "Práctica")]
        Practica = 2,

        [Display(Name = "Examen")]
        Examen = 3,

        [Display(Name = "Proyecto")]
        Proyecto = 4,

        [Display(Name = "Laboratorio")]
        Laboratorio = 5
    }
}