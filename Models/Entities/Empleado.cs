using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Empleado
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        [Required(ErrorMessage = "El CI es requerido")]
        [StringLength(20)]
        [Display(Name = "Cédula de Identidad")]
        public string CI { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El cargo es requerido")]
        [StringLength(100)]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [Display(Name = "Fecha de Contratación")]
        [DataType(DataType.Date)]
        public DateTime FechaContratacion { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de Baja")]
        [DataType(DataType.Date)]
        public DateTime? FechaBaja { get; set; }

        // Relación con Identity User
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}