namespace SistemaGestionAcademica.Models.DTOs
{
    public class HorarioCompletoDTO
    {
        public int EstudianteId { get; set; }
        public List<HorarioMateriaDTO> Materias { get; set; } = new();
    }

    public class HorarioMateriaDTO
    {
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string Dia { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
    }
}