using Microsoft.EntityFrameworkCore;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    public class MateriaRepository : GenericRepository<Materia>, IMateriaRepository
    {
        public MateriaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Materia>> GetMateriasDisponiblesAsync()
        {
            return await _dbSet
                .Include(m => m.Profesor)
                .Include(m => m.Aula)
                .Include(m => m.Horario)
                .Include(m => m.Inscripciones)
                .Where(m => m.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Materia>> GetMateriasPorProfesorAsync(int profesorId)
        {
            return await _dbSet
                .Include(m => m.Inscripciones)
                .Where(m => m.ProfesorId == profesorId && m.Activo)
                .ToListAsync();
        }

        public async Task<Materia?> GetMateriaConDetallesAsync(int materiaId)
        {
            return await _dbSet
                .Include(m => m.Profesor)
                .Include(m => m.Aula)
                .Include(m => m.Horario)
                .Include(m => m.Inscripciones)
                .ThenInclude(i => i.Estudiante)
                .Include(m => m.Actividades)
                .FirstOrDefaultAsync(m => m.Id == materiaId);
        }

        public async Task<IEnumerable<Estudiante>> GetEstudiantesInscritosAsync(int materiaId)
        {
            return await _context.Inscripciones
                .Include(i => i.Estudiante)
                .Where(i => i.MateriaId == materiaId && i.Estado == EstadoInscripcion.Activa)
                .Select(i => i.Estudiante)
                .ToListAsync();
        }

        public async Task<int> GetCantidadInscritosAsync(int materiaId)
        {
            return await _context.Inscripciones
                .CountAsync(i => i.MateriaId == materiaId && i.Estado == EstadoInscripcion.Activa);
        }
    }
}