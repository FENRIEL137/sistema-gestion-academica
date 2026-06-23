using SistemaGestionAcademica.Models.Entities;

namespace SistemaGestionAcademica.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el patrón Unit of Work
    /// Centraliza el acceso a todos los repositorios y la transacción
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repositorios específicos
        IEstudianteRepository Estudiantes { get; }
        IProfesorRepository Profesores { get; }
        IEmpleadoRepository Empleados { get; }
        IMateriaRepository Materias { get; }
        IAulaRepository Aulas { get; }
        IHorarioRepository Horarios { get; }
        IInscripcionRepository Inscripciones { get; }
        IPagoRepository Pagos { get; }
        IActividadRepository Actividades { get; }
        INotaRepository Notas { get; }
        IConfiguracionInstitucionalRepository Configuraciones { get; }

        // Métodos de transacción
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}