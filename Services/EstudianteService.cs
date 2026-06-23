using SistemaGestionAcademica.Models.DTOs;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Services
{
    public class EstudianteService : IEstudianteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EstudianteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ============ INSCRIPCIONES ============

        public async Task<IEnumerable<MateriaDTO>> GetMateriasDisponiblesAsync()
        {
            var materias = await _unitOfWork.Materias.GetMateriasDisponiblesAsync();

            return materias.Select(m => new MateriaDTO
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Costo = m.Costo,
                ProfesorNombre = m.Profesor?.NombreCompleto ?? "Sin asignar",
                Aula = m.Aula?.Nombre ?? "Sin asignar",
                Horario = m.Horario?.HorarioCompleto ?? "Sin asignar",
                CuposDisponibles = (m.Aula?.Capacidad ?? 0) - m.Inscripciones.Count
            });
        }

        public async Task<bool> InscribirMateriaAsync(int estudianteId, int materiaId)
        {
            var tienePagoInicial = await _unitOfWork.Estudiantes.TienePagoInicialAsync(estudianteId);
            if (!tienePagoInicial)
                throw new InvalidOperationException("Debe realizar el pago inicial antes de inscribirse");

            var estaInscrito = await _unitOfWork.Inscripciones.EstaInscritoAsync(estudianteId, materiaId);
            if (estaInscrito)
                throw new InvalidOperationException("Ya esta inscrito en esta materia");

            var materia = await _unitOfWork.Materias.GetMateriaConDetallesAsync(materiaId);
            if (materia?.Aula != null && materia.Inscripciones.Count >= materia.Aula.Capacidad)
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

            return true;
        }

        public async Task<IEnumerable<InscripcionDTO>> GetMateriasInscritasAsync(int estudianteId)
        {
            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(estudianteId);

            return inscripciones.Select(i => new InscripcionDTO
            {
                Id = i.Id,
                MateriaId = i.MateriaId,
                MateriaNombre = i.Materia?.Nombre ?? "Sin nombre",
                ProfesorNombre = i.Materia?.Profesor?.NombreCompleto ?? "Sin asignar",
                Aula = i.Materia?.Aula?.Nombre ?? "Sin asignar",
                Horario = i.Materia?.Horario?.HorarioCompleto ?? "Sin asignar",
                Costo = i.Materia?.Costo ?? 0,
                PagoRealizado = i.PagoRealizado,
                NotaFinal = i.NotaFinal,
                Estado = i.Estado.ToString()
            });
        }

        public async Task<HorarioCompletoDTO> GetHorarioEstudianteAsync(int estudianteId)
        {
            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(estudianteId);

            return new HorarioCompletoDTO
            {
                EstudianteId = estudianteId,
                Materias = inscripciones.Select(i => new HorarioMateriaDTO
                {
                    MateriaId = i.MateriaId,
                    MateriaNombre = i.Materia?.Nombre ?? "Sin nombre",
                    Dia = i.Materia?.Horario?.Dia.ToString() ?? "No definido",
                    HoraInicio = i.Materia?.Horario?.HoraInicio.ToString(@"hh\:mm") ?? "",
                    HoraFin = i.Materia?.Horario?.HoraFin.ToString(@"hh\:mm") ?? "",
                    Aula = i.Materia?.Aula?.Nombre ?? "Sin asignar",
                    Profesor = i.Materia?.Profesor?.NombreCompleto ?? "Sin asignar"
                }).ToList()
            };
        }

        // ============ PAGOS - METODO CORREGIDO ============

        public async Task<bool> RealizarPagoAsync(int estudianteId, int inscripcionId, decimal monto)
        {
            // USAR GetInscripcionConDetallesAsync para cargar la Materia
            var inscripcion = await _unitOfWork.Inscripciones.GetInscripcionConDetallesAsync(inscripcionId);

            if (inscripcion == null)
                throw new InvalidOperationException("Inscripcion no valida");

            if (inscripcion.EstudianteId != estudianteId)
                throw new InvalidOperationException("La inscripcion no pertenece a este estudiante");

            // VERIFICAR que Materia no sea null
            if (inscripcion.Materia == null)
                throw new InvalidOperationException("Error: La inscripcion no tiene materia asociada");

            var pago = new Pago
            {
                InscripcionId = inscripcionId,
                EstudianteId = estudianteId,
                Monto = monto,
                FechaPago = DateTime.Now,
                Tipo = TipoPago.Materia,
                Estado = EstadoPago.Completado
            };

            await _unitOfWork.Pagos.AddAsync(pago);
            await _unitOfWork.CompleteAsync();

            // Obtener total pagado (incluyendo el pago recien hecho)
            var pagosRealizados = await _unitOfWork.Pagos.GetPagosPorInscripcionAsync(inscripcionId);
            var totalPagado = pagosRealizados.Sum(p => p.Monto);

            // Actualizar estado de pago
            if (totalPagado >= inscripcion.Materia.Costo)
            {
                inscripcion.PagoRealizado = true;
                await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
                await _unitOfWork.CompleteAsync();
            }

            return true;
        }

        public async Task<IEnumerable<PagoDTO>> GetHistorialPagosAsync(int estudianteId)
        {
            var pagos = await _unitOfWork.Estudiantes.GetHistorialPagosAsync(estudianteId);

            return pagos.Select(p => new PagoDTO
            {
                Id = p.Id,
                Monto = p.Monto,
                FechaPago = p.FechaPago,
                Tipo = p.Tipo.ToString(),
                Concepto = p.Concepto ?? "Pago de " + p.Tipo.ToString(),
                Estado = p.Estado.ToString()
            });
        }

        public async Task<decimal> GetDeudaTotalAsync(int estudianteId)
        {
            return await _unitOfWork.Estudiantes.GetDeudaTotalAsync(estudianteId);
        }

        public async Task<IEnumerable<DeudaDTO>> GetDeudasPendientesAsync(int estudianteId)
        {
            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(estudianteId);

            return inscripciones
                .Where(i => !i.PagoRealizado && i.Estado == EstadoInscripcion.Activa)
                .Select(i => new DeudaDTO
                {
                    InscripcionId = i.Id,
                    MateriaNombre = i.Materia?.Nombre ?? "Sin nombre",
                    Monto = i.Materia?.Costo ?? 0,
                    FechaInscripcion = i.FechaInscripcion
                });
        }

        // ============ CONSULTAS ============

        public async Task<IEnumerable<NotaDTO>> GetNotasAsync(int estudianteId)
        {
            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(estudianteId);
            var notasList = new List<NotaDTO>();

            foreach (var inscripcion in inscripciones)
            {
                var notas = await _unitOfWork.Notas.GetNotasPorInscripcionAsync(inscripcion.Id);
                notasList.AddRange(notas.Select(n => new NotaDTO
                {
                    Id = n.Id,
                    MateriaNombre = inscripcion.Materia?.Nombre ?? "Sin nombre",
                    ActividadNombre = n.Actividad?.Nombre ?? "Sin nombre",
                    TipoActividad = n.Actividad?.Tipo.ToString() ?? "Sin tipo",
                    Calificacion = n.Calificacion,
                    ValorPorcentual = n.Actividad?.ValorPorcentual ?? 0
                }));
            }

            return notasList;
        }

        public async Task<IEnumerable<ActividadDTO>> GetActividadesAsync(int estudianteId, int materiaId)
        {
            var actividades = await _unitOfWork.Actividades.GetActividadesPorMateriaAsync(materiaId);

            return actividades.Select(a => new ActividadDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Fecha = a.Fecha,
                Tipo = a.Tipo.ToString(),
                ValorPorcentual = a.ValorPorcentual
            });
        }

        public async Task<EstadoAcademicoDTO> GetEstadoAcademicoAsync(int estudianteId)
        {
            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(estudianteId);

            var materiasEstado = new List<MateriaEstadoDTO>();
            foreach (var inscripcion in inscripciones)
            {
                var notaFinal = await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(inscripcion.Id);

                materiasEstado.Add(new MateriaEstadoDTO
                {
                    MateriaId = inscripcion.MateriaId,
                    MateriaNombre = inscripcion.Materia?.Nombre ?? "Sin nombre",
                    NotaFinal = notaFinal,
                    Estado = notaFinal >= 51 ? "Aprobado" : "Reprobado",
                    PagoRealizado = inscripcion.PagoRealizado
                });
            }

            var promedioGeneral = materiasEstado.Any()
                ? materiasEstado.Average(m => m.NotaFinal ?? 0)
                : 0;

            return new EstadoAcademicoDTO
            {
                EstudianteId = estudianteId,
                TotalMaterias = materiasEstado.Count,
                MateriasAprobadas = materiasEstado.Count(m => m.Estado == "Aprobado"),
                PromedioGeneral = promedioGeneral,
                Materias = materiasEstado
            };
        }
    }
}