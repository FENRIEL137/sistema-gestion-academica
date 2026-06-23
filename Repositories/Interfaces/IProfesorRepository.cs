using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IProfesorRepository : IGenericRepository<Profesor>
    {
        Task<Profesor?> GetByCIAsync(string ci);
        Task<Profesor?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Profesor>> GetProfesoresActivosAsync();
        Task<IEnumerable<Materia>> GetMateriasAsignadasAsync(int profesorId);
        Task<IEnumerable<Estudiante>> GetEstudiantesPorMateriaAsync(int materiaId);
    }
}