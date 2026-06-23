using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface INotaRepository : IGenericRepository<Nota>
    {
        /// <summary>
        /// Obtiene todas las notas de una inscripción
        /// </summary>
        Task<IEnumerable<Nota>> GetNotasPorInscripcionAsync(int inscripcionId);

        /// <summary>
        /// Obtiene todas las notas de una actividad específica
        /// </summary>
        Task<IEnumerable<Nota>> GetNotasPorActividadAsync(int actividadId);

        /// <summary>
        /// Obtiene una nota específica (inscripción + actividad)
        /// </summary>
        Task<Nota?> GetNotaAsync(int inscripcionId, int actividadId);

        /// <summary>
        /// Obtiene un diccionario de notas por actividad para una inscripción
        /// Retorna: Key = ActividadId, Value = Calificacion
        /// </summary>
        Task<Dictionary<int, decimal>> GetDiccionarioNotasPorActividadAsync(int inscripcionId);
    }
}