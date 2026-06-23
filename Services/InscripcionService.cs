using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Services
{
    public class InscripcionService : IInscripcionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InscripcionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Inscripcion> InscribirAsync(int estudianteId, int materiaId)
        {
            // Verificar pago inicial
            if (!await _unitOfWork.Estudiantes.TienePagoInicialAsync(estudianteId))
                throw new InvalidOperationException("Debe realizar el pago inicial antes de inscribirse");

            // Verificar si ya está inscrito
            if (await _unitOfWork.Inscripciones.EstaInscritoAsync(estudianteId, materiaId))
                throw new InvalidOperationException("Ya está inscrito en esta materia");

            // Verificar cupos
            if (!await VerificarDisponibilidadAsync(materiaId))
                throw new InvalidOperationException("No hay cupos disponibles");

            var inscripcion = new Inscripcion
            {
                EstudianteId = estudianteId,
                MateriaId = materiaId,
                FechaInscripcion = DateTime.Now,
                Estado = EstadoInscripcion.Activa,
                PagoRealizado = false
            };

            await _unitOfWork.Inscripciones.AddAsync(inscripcion);
            await _unitOfWork.CompleteAsync();

            return inscripcion;
        }

        public async Task<bool> CancelarInscripcionAsync(int inscripcionId)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(inscripcionId);
            if (inscripcion == null) return false;

            inscripcion.Estado = EstadoInscripcion.BajaDefinitiva;
            inscripcion.FechaBaja = DateTime.Now;
            await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ReactivarInscripcionAsync(int inscripcionId)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(inscripcionId);
            if (inscripcion == null) return false;

            inscripcion.Estado = EstadoInscripcion.Activa;
            inscripcion.FechaBaja = null;
            await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<Inscripcion>> GetInscripcionesActivasAsync(int estudianteId)
        {
            return await _unitOfWork.Inscripciones.GetInscripcionesPorEstudianteAsync(estudianteId);
        }

        public async Task<bool> VerificarDisponibilidadAsync(int materiaId)
        {
            var materia = await _unitOfWork.Materias.GetMateriaConDetallesAsync(materiaId);
            if (materia?.Aula == null) return false;

            var inscritos = await _unitOfWork.Materias.GetCantidadInscritosAsync(materiaId);
            return inscritos < materia.Aula.Capacidad;
        }
    }
}