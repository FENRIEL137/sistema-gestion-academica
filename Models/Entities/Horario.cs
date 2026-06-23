using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAcademica.Models.Entities
{
    public class Horario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El día es requerido")]
        [Display(Name = "Día")]
        public DayOfWeek Dia { get; set; }

        [Required(ErrorMessage = "La hora de inicio es requerida")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es requerida")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Fin")]
        public TimeSpan HoraFin { get; set; }

        [NotMapped]
        [Display(Name = "Horario Completo")]
        public string HorarioCompleto => $"{Dia} {HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<Materia> Materias { get; set; } = new List<Materia>();
    }
}