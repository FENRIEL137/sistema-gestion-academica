using Microsoft.EntityFrameworkCore.Storage;
using SistemaGestionAcademica.Data;
using SistemaGestionAcademica.Repositories.Interfaces;

namespace SistemaGestionAcademica.Repositories
{
    /// <summary>
    /// Implementación del patrón Unit of Work
    /// Gestiona la transacción y centraliza el acceso a repositorios
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repositorios (Lazy Loading)
        private IEstudianteRepository? _estudiantes;
        private IProfesorRepository? _profesores;
        private IEmpleadoRepository? _empleados;
        private IMateriaRepository? _materias;
        private IAulaRepository? _aulas;
        private IHorarioRepository? _horarios;
        private IInscripcionRepository? _inscripciones;
        private IPagoRepository? _pagos;
        private IActividadRepository? _actividades;
        private INotaRepository? _notas;
        private IConfiguracionInstitucionalRepository? _configuraciones;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Propiedades de repositorios
        public IEstudianteRepository Estudiantes =>
            _estudiantes ??= new EstudianteRepository(_context);

        public IProfesorRepository Profesores =>
            _profesores ??= new ProfesorRepository(_context);

        public IEmpleadoRepository Empleados =>
            _empleados ??= new EmpleadoRepository(_context);

        public IMateriaRepository Materias =>
            _materias ??= new MateriaRepository(_context);

        public IAulaRepository Aulas =>
            _aulas ??= new AulaRepository(_context);

        public IHorarioRepository Horarios =>
            _horarios ??= new HorarioRepository(_context);

        public IInscripcionRepository Inscripciones =>
            _inscripciones ??= new InscripcionRepository(_context);

        public IPagoRepository Pagos =>
            _pagos ??= new PagoRepository(_context);

        public IActividadRepository Actividades =>
            _actividades ??= new ActividadRepository(_context);

        public INotaRepository Notas =>
            _notas ??= new NotaRepository(_context);

        public IConfiguracionInstitucionalRepository Configuraciones =>
            _configuraciones ??= new ConfiguracionInstitucionalRepository(_context);

        // Métodos de transacción
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}