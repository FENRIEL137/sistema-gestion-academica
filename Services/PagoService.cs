using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Services
{
    public class PagoService : IPagoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PagoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Pago> ProcesarPagoAsync(int estudianteId, int inscripcionId, decimal monto, string concepto)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetInscripcionConDetallesAsync(inscripcionId)
                ?? throw new InvalidOperationException("Inscripción no encontrada");

            if (inscripcion.EstudianteId != estudianteId)
                throw new InvalidOperationException("La inscripción no pertenece a este estudiante");

            // Verificar si hay penalización por mora
            var config = await _unitOfWork.Configuraciones.GetConfiguracionActualAsync();
            var diasAtraso = (DateTime.UtcNow - inscripcion.FechaInscripcion).Days;
            decimal montoFinal = monto;

            if (diasAtraso > 30 && config != null)
            {
                var penalizacion = monto * (config.PorcentajePenalizacionMora / 100);
                montoFinal += penalizacion;
            }

            var pago = new Pago
            {
                InscripcionId = inscripcionId,
                EstudianteId = estudianteId,
                Monto = montoFinal,
                FechaPago = DateTime.UtcNow,
                Tipo = TipoPago.Materia,
                Concepto = concepto,
                Estado = EstadoPago.Completado
            };

            await _unitOfWork.Pagos.AddAsync(pago);

            // Actualizar estado de pago
            var totalPagado = (await _unitOfWork.Pagos.GetTotalPagadoAsync(estudianteId)) + montoFinal;
            if (totalPagado >= inscripcion.Materia.Costo)
            {
                inscripcion.PagoRealizado = true;
                await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            }

            await _unitOfWork.CompleteAsync();
            return pago;
        }

        public async Task<IEnumerable<Pago>> GetHistorialPagosAsync(int estudianteId)
        {
            return await _unitOfWork.Pagos.GetPagosPorEstudianteAsync(estudianteId);
        }

        public async Task<decimal> GetDeudaTotalAsync(int estudianteId)
        {
            return await _unitOfWork.Pagos.GetDeudaPendienteAsync(estudianteId);
        }

        public async Task<bool> VerificarPagoInicialAsync(int estudianteId)
        {
            return await _unitOfWork.Estudiantes.TienePagoInicialAsync(estudianteId);
        }

        public async Task<bool> EsPeriodoPagoAsync()
        {
            var config = await _unitOfWork.Configuraciones.GetConfiguracionActualAsync();
            if (config == null) return false;

            var hoy = DateTime.UtcNow;
            var inicio = new DateTime(hoy.Year, hoy.Month, config.DiaInicioPagos);
            var fin = new DateTime(hoy.Year, hoy.Month, config.DiaFinPagos);

            return hoy >= inicio && hoy <= fin;
        }

        public async Task AplicarPenalizacionMoraAsync()
        {
            var config = await _unitOfWork.Configuraciones.GetConfiguracionActualAsync();
            if (config == null) return;

            var inscripcionesPendientes = await _unitOfWork.Inscripciones.GetInscripcionesPendientesPagoAsync();
            var hoy = DateTime.UtcNow;

            foreach (var inscripcion in inscripcionesPendientes)
            {
                var diasAtraso = (hoy - inscripcion.FechaInscripcion).Days;
                if (diasAtraso > 30)
                {
                    var mora = inscripcion.Materia.Costo * (config.PorcentajePenalizacionMora / 100);

                    var pagoMora = new Pago
                    {
                        InscripcionId = inscripcion.Id,
                        EstudianteId = inscripcion.EstudianteId,
                        Monto = mora,
                        FechaPago = hoy,
                        Tipo = TipoPago.Penalizacion,
                        Concepto = $"Penalización por mora - {diasAtraso} días",
                        Estado = EstadoPago.Pendiente
                    };

                    await _unitOfWork.Pagos.AddAsync(pagoMora);
                }
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}