using Microsoft.AspNetCore.Identity;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmpleadoService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<Estudiante> RegistrarEstudianteAsync(Estudiante estudiante, string email, string password)
        {
            if (await _unitOfWork.Estudiantes.GetByCIAsync(estudiante.CI) != null)
                throw new InvalidOperationException("Ya existe un estudiante con esta cédula");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NombreCompleto = $"{estudiante.Nombre} {estudiante.Apellido}",
                EmailConfirmed = true,
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Estudiante");

            estudiante.UserId = user.Id;
            estudiante.FechaInscripcion = DateTime.Now;
            estudiante.Activo = true;

            await _unitOfWork.Estudiantes.AddAsync(estudiante);
            await _unitOfWork.CompleteAsync();

            return estudiante;
        }

        public async Task<Estudiante> ActualizarEstudianteAsync(Estudiante estudiante)
        {
            await _unitOfWork.Estudiantes.UpdateAsync(estudiante);
            await _unitOfWork.CompleteAsync();
            return estudiante;
        }

        public async Task<bool> DarBajaEstudianteAsync(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            if (estudiante == null) return false;

            estudiante.Activo = false;
            estudiante.FechaBaja = DateTime.Now;

            // Dar de baja inscripciones activas
            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(id);
            foreach (var inscripcion in inscripciones.Where(i => i.Estado == EstadoInscripcion.Activa))
            {
                inscripcion.Estado = EstadoInscripcion.BajaDefinitiva;
                inscripcion.FechaBaja = DateTime.Now;
                await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            }

            await _unitOfWork.Estudiantes.UpdateAsync(estudiante);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ReactivarEstudianteAsync(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            if (estudiante == null) return false;

            estudiante.Activo = true;
            estudiante.FechaBaja = null;
            await _unitOfWork.Estudiantes.UpdateAsync(estudiante);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<Pago> RegistrarPagoAsync(int inscripcionId, decimal monto, string concepto, string usuarioId)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(inscripcionId)
                ?? throw new InvalidOperationException("Inscripción no encontrada");

            var pago = new Pago
            {
                InscripcionId = inscripcionId,
                EstudianteId = inscripcion.EstudianteId,
                Monto = monto,
                FechaPago = DateTime.Now,
                Tipo = TipoPago.Materia,
                Concepto = concepto,
                RegistradoPorId = usuarioId,
                Estado = EstadoPago.Completado
            };

            await _unitOfWork.Pagos.AddAsync(pago);

            // Verificar si el pago cubre el total
            var totalPagado = (await _unitOfWork.Pagos.GetPagosPorInscripcionAsync(inscripcionId))
                .Sum(p => p.Monto);

            if (totalPagado >= inscripcion.Materia.Costo)
            {
                inscripcion.PagoRealizado = true;
                await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            }

            await _unitOfWork.CompleteAsync();
            return pago;
        }

        public async Task<Inscripcion> RegistrarInscripcionAsync(int estudianteId, int materiaId)
        {
            if (await _unitOfWork.Inscripciones.EstaInscritoAsync(estudianteId, materiaId))
                throw new InvalidOperationException("El estudiante ya está inscrito en esta materia");

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

        public async Task<bool> DarBajaInscripcionAsync(int id)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(id);
            if (inscripcion == null) return false;

            inscripcion.Estado = EstadoInscripcion.BajaTemporal;
            inscripcion.FechaBaja = DateTime.Now;
            await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> EsPeriodoPagoAsync()
        {
            var config = await _unitOfWork.Configuraciones.GetConfiguracionActualAsync();
            if (config == null) return false;

            var hoy = DateTime.Now;
            var inicio = new DateTime(hoy.Year, hoy.Month, config.DiaInicioPagos);
            var fin = new DateTime(hoy.Year, hoy.Month, config.DiaFinPagos);

            return hoy >= inicio && hoy <= fin;
        }
    }
}