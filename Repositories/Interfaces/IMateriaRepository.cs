using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IMateriaRepository : IGenericRepository<Materia>
    {
        Task<IEnumerable<Materia>> GetMateriasDisponiblesAsync();
        Task<IEnumerable<Materia>> GetMateriasPorProfesorAsync(int profesorId);
        Task<Materia?> GetMateriaConDetallesAsync(int materiaId);
        Task<IEnumerable<Estudiante>> GetEstudiantesInscritosAsync(int materiaId);
        Task<int> GetCantidadInscritosAsync(int materiaId);
    }
}