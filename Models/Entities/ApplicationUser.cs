using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAcademica.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(200)]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Último Acceso")]
        public DateTime? UltimoAcceso { get; set; }

        // Relaciones con las entidades específicas
        public virtual Estudiante? Estudiante { get; set; }
        public virtual Profesor? Profesor { get; set; }
        public virtual Empleado? Empleado { get; set; }
    }
}