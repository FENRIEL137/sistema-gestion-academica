using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Inscripcion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Estudiante")]
        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante Estudiante { get; set; } = null!;

        [Required]
        [Display(Name = "Materia")]
        public int MateriaId { get; set; }

        [ForeignKey("MateriaId")]
        public virtual Materia Materia { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inscripción")]
        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Estado")]
        public EstadoInscripcion Estado { get; set; } = EstadoInscripcion.Activa;

        [Display(Name = "Pago Realizado")]
        public bool PagoRealizado { get; set; } = false;

        [Display(Name = "Nota Final")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? NotaFinal { get; set; }

        [Display(Name = "Fecha de Baja")]
        [DataType(DataType.Date)]
        public DateTime? FechaBaja { get; set; }

        // Relaciones
        public virtual ICollection<Nota> Notas { get; set; } = new List<Nota>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }

    public enum EstadoInscripcion
    {
        [Display(Name = "Activa")]
        Activa = 1,

        [Display(Name = "Baja Temporal")]
        BajaTemporal = 2,

        [Display(Name = "Baja Definitiva")]
        BajaDefinitiva = 3,

        [Display(Name = "Completada")]
        Completada = 4
    }
}