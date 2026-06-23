using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    public interface IHorarioRepository : IGenericRepository<Horario>
    {
        Task<IEnumerable<Horario>> GetHorariosDisponiblesAsync();
    }
}