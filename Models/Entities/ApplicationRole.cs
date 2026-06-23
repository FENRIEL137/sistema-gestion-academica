using Microsoft.AspNetCore.Identity;

namespace SistemaGestionAcademica.Models.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}