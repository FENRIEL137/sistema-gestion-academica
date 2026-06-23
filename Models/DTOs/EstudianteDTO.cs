namespace SistemaGestionAcademica.Models.DTOs
{
    public class EstudianteDTO
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CI { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }
}