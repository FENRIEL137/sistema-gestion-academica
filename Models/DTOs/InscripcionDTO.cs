namespace SistemaGestionAcademica.Models.DTOs
{
    public class InscripcionDTO
    {
        public int Id { get; set; }
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string ProfesorNombre { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public decimal Costo { get; set; }
        public bool PagoRealizado { get; set; }
        public decimal? NotaFinal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}