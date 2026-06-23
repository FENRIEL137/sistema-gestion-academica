namespace SistemaGestionAcademica.Models.DTOs
{
    public class EstadoAcademicoDTO
    {
        public int EstudianteId { get; set; }
        public int TotalMaterias { get; set; }
        public int MateriasAprobadas { get; set; }
        public int MateriasReprobadas => TotalMaterias - MateriasAprobadas;
        public decimal PromedioGeneral { get; set; }
        public List<MateriaEstadoDTO> Materias { get; set; } = new();
    }

    public class MateriaEstadoDTO
    {
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public decimal? NotaFinal { get; set; }
        public string Estado { get; set; } = "Sin calificar";
        public bool PagoRealizado { get; set; }
    }
}