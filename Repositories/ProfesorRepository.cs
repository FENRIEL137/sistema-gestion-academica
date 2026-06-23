using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class ProfesorRepository : GenericRepository<Profesor>, IProfesorRepository
    {
        public ProfesorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Profesor?> GetByCIAsync(string ci)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.CI == ci);
        }

        public async Task<Profesor?> GetByUserIdAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<Profesor>> GetProfesoresActivosAsync()
        {
            return await _dbSet
                .Where(p => p.Activo)
                .Include(p => p.Materias)
                .ToListAsync();
        }

        public async Task<IEnumerable<Materia>> GetMateriasAsignadasAsync(int profesorId)
        {
            return await _context.Materias
                .Include(m => m.Aula)
                .Include(m => m.Horario)
                .Include(m => m.Inscripciones)
                .ThenInclude(i => i.Estudiante)
                .Where(m => m.ProfesorId == profesorId && m.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Estudiante>> GetEstudiantesPorMateriaAsync(int materiaId)
        {
            return await _context.Inscripciones
                .Include(i => i.Estudiante)
                .Where(i => i.MateriaId == materiaId && i.Estado == EstadoInscripcion.Activa)
                .Select(i => i.Estudiante)
                .ToListAsync();
        }
    }
}