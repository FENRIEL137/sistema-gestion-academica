using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Inscripción")]
        public int? InscripcionId { get; set; }

        [ForeignKey("InscripcionId")]
        public virtual Inscripcion? Inscripcion { get; set; }

        [Display(Name = "Estudiante")]
        public int? EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante? Estudiante { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Tipo de Pago")]
        public TipoPago Tipo { get; set; }

        [Display(Name = "Concepto")]
        [StringLength(200)]
        public string? Concepto { get; set; }

        [Display(Name = "Comprobante")]
        [StringLength(50)]
        public string? Comprobante { get; set; }

        [Display(Name = "Registrado Por")]
        public string? RegistradoPorId { get; set; }

        [ForeignKey("RegistradoPorId")]
        public virtual ApplicationUser? RegistradoPor { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Estado")]
        public EstadoPago Estado { get; set; } = EstadoPago.Completado;
    }

    public enum TipoPago
    {
        [Display(Name = "Inscripción Inicial")]
        InscripcionInicial = 1,

        [Display(Name = "Mensualidad")]
        Mensualidad = 2,

        [Display(Name = "Materia")]
        Materia = 3,

        [Display(Name = "Penalización")]
        Penalizacion = 4,

        [Display(Name = "Otro")]
        Otro = 5
    }

    public enum EstadoPago
    {
        [Display(Name = "Pendiente")]
        Pendiente = 1,

        [Display(Name = "Completado")]
        Completado = 2,

        [Display(Name = "Anulado")]
        Anulado = 3,

        [Display(Name = "Con Mora")]
        ConMora = 4
    }
}