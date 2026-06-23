using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class ConfiguracionInstitucional
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nombre de la Institución")]
        [StringLength(200)]
        public string NombreInstitucion { get; set; } = "Mi Institución Educativa";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Pago Inicial de Inscripción")]
        public decimal PagoInicialInscripcion { get; set; } = 500.00m;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Costo Base por Materia")]
        public decimal CostoBaseMateria { get; set; } = 300.00m;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Porcentaje de Penalización por Mora")]
        public decimal PorcentajePenalizacionMora { get; set; } = 5.00m;

        [Required]
        [Range(1, 30, ErrorMessage = "El día de inicio de pagos debe estar entre 1 y 30")]
        [Display(Name = "Día de Inicio de Pagos")]
        public int DiaInicioPagos { get; set; } = 23;

        [Required]
        [Range(1, 30, ErrorMessage = "El día de fin de pagos debe estar entre 1 y 30")]
        [Display(Name = "Día de Fin de Pagos")]
        public int DiaFinPagos { get; set; } = 30;

        [Display(Name = "Periodo Actual")]
        [StringLength(50)]
        public string? PeriodoActual { get; set; } = "2026-I";

        [Display(Name = "Fecha de Última Actualización")]
        public DateTime FechaUltimaActualizacion { get; set; } = DateTime.Now;

        [Display(Name = "Actualizado Por")]
        public string? ActualizadoPorId { get; set; }

        [ForeignKey("ActualizadoPorId")]
        public virtual ApplicationUser? ActualizadoPor { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}