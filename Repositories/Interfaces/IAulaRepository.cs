using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IAulaRepository : IGenericRepository<Aula>
    {
        Task<IEnumerable<Aula>> GetAulasDisponiblesAsync();
        Task<IEnumerable<Aula>> GetLaboratoriosAsync();
        Task<bool> EstaAulaDisponibleAsync(int aulaId, int horarioId);
    }
}