namespace SistemaGestionAcademica.Models.DTOs
{
    public class MateriaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Costo { get; set; }
        public string ProfesorNombre { get; set; } = "Sin asignar";
        public string Aula { get; set; } = "Sin asignar";
        public string Horario { get; set; } = "Sin asignar";
        public int CuposDisponibles { get; set; }
    }
}