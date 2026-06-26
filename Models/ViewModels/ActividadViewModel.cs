using System.ComponentModel.DataAnnotations;
using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Models.ViewModels
{
    public class ActividadViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(150)]
        [Display(Name = "Nombre de la Actividad")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El tipo de actividad es requerido")]
        [Display(Name = "Tipo de Actividad")]
        public TipoActividad Tipo { get; set; }

        [Required(ErrorMessage = "El valor porcentual es requerido")]
        [Range(0, 100, ErrorMessage = "El valor debe estar entre 0 y 100")]
        [Display(Name = "Valor Porcentual")]
        public decimal ValorPorcentual { get; set; }

        public int MateriaId { get; set; }
        public string? MateriaNombre { get; set; }
    }
}