using ClosedXML.Excel;
using SistemaGestionAcademica.Models.DTOs;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Models.ViewModels;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Services
{
    public class ProfesorService : IProfesorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProfesorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MateriaDTO>> GetMateriasAsignadasAsync(int profesorId)
        {
            var materias = await _unitOfWork.Profesores.GetMateriasAsignadasAsync(profesorId);

            return materias.Select(m => new MateriaDTO
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Costo = m.Costo,
                ProfesorNombre = m.Profesor?.NombreCompleto ?? "",
                Aula = m.Aula?.Nombre ?? "",
                Horario = m.Horario?.HorarioCompleto ?? "",
                CuposDisponibles = m.Aula?.Capacidad - m.Inscripciones.Count ?? 0
            });
        }

        public async Task<IEnumerable<EstudianteDTO>> GetEstudiantesInscritosAsync(int materiaId)
        {
            var estudiantes = await _unitOfWork.Profesores.GetEstudiantesPorMateriaAsync(materiaId);

            return estudiantes.Select(e => new EstudianteDTO
            {
                Id = e.Id,
                NombreCompleto = e.NombreCompleto,
                CI = e.CI,
                Telefono = e.Telefono,
                Correo = e.Correo
            });
        }

        public async Task<IEnumerable<ActividadDTO>> GetActividadesAsync(int materiaId)
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

        public async Task<ActividadDTO> CrearActividadAsync(ActividadViewModel model)
        {
            if (!await _unitOfWork.Actividades.ValidarPorcentajesAsync(model.MateriaId, model.ValorPorcentual))
                throw new InvalidOperationException("La suma de porcentajes no puede exceder 100%");

            var actividad = new Actividad
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Fecha = model.Fecha.ToUniversalTime(), // Convertir a UTC
                Tipo = model.Tipo,
                ValorPorcentual = model.ValorPorcentual,
                MateriaId = model.MateriaId,
                FechaCreacion = DateTime.UtcNow,       // Usar UTC
                Activo = true
            };

            await _unitOfWork.Actividades.AddAsync(actividad);
            await _unitOfWork.CompleteAsync();

            return new ActividadDTO
            {
                Id = actividad.Id,
                Nombre = actividad.Nombre,
                Descripcion = actividad.Descripcion,
                Fecha = actividad.Fecha,
                Tipo = actividad.Tipo.ToString(),
                ValorPorcentual = actividad.ValorPorcentual
            };
        }

        public async Task<ActividadDTO> ActualizarActividadAsync(ActividadViewModel model)
        {
            var actividad = await _unitOfWork.Actividades.GetByIdAsync(model.Id);
            if (actividad == null)
                throw new InvalidOperationException("Actividad no encontrada");

            if (!await _unitOfWork.Actividades.ValidarPorcentajesAsync(model.MateriaId, model.ValorPorcentual, model.Id))
                throw new InvalidOperationException("La suma de porcentajes no puede exceder 100%");

            actividad.Nombre = model.Nombre;
            actividad.Descripcion = model.Descripcion;
            actividad.Fecha = model.Fecha;
            actividad.Tipo = model.Tipo;
            actividad.ValorPorcentual = model.ValorPorcentual;

            await _unitOfWork.Actividades.UpdateAsync(actividad);
            await _unitOfWork.CompleteAsync();

            return new ActividadDTO
            {
                Id = actividad.Id,
                Nombre = actividad.Nombre,
                Descripcion = actividad.Descripcion,
                Fecha = actividad.Fecha,
                Tipo = actividad.Tipo.ToString(),
                ValorPorcentual = actividad.ValorPorcentual
            };
        }

        public async Task<bool> EliminarActividadAsync(int actividadId)
        {
            var actividad = await _unitOfWork.Actividades.GetByIdAsync(actividadId);
            if (actividad == null) return false;

            actividad.Activo = false;
            await _unitOfWork.Actividades.UpdateAsync(actividad);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<NotaViewModel> GetVistaNotasAsync(int materiaId)
        {
            var materia = await _unitOfWork.Materias.GetMateriaConDetallesAsync(materiaId);
            if (materia == null)
                throw new InvalidOperationException("Materia no encontrada");

            var actividades = await _unitOfWork.Actividades.GetActividadesPorMateriaAsync(materiaId);
            var inscripciones = await _unitOfWork.Inscripciones.GetInscripcionesPorMateriaAsync(materiaId);

            var viewModel = new NotaViewModel
            {
                MateriaId = materiaId,
                MateriaNombre = materia.Nombre,
                ActividadesNombres = actividades.Select(a => a.Nombre).ToList()
            };

            foreach (var inscripcion in inscripciones)
            {
                // CORRECCIÓN: Usar el nuevo nombre del método
                var notasDict = await _unitOfWork.Notas.GetDiccionarioNotasPorActividadAsync(inscripcion.Id);
                var notaFinal = await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(inscripcion.Id);

                var estudianteNota = new EstudianteNotaViewModel
                {
                    InscripcionId = inscripcion.Id,
                    EstudianteId = inscripcion.EstudianteId,
                    EstudianteCI = inscripcion.Estudiante.CI,
                    EstudianteNombre = inscripcion.Estudiante.NombreCompleto,
                    Notas = notasDict.ToDictionary(
                        n => n.Key,
                        n => (decimal?)n.Value
                    ),
                    NotaFinal = notaFinal
                };

                viewModel.Estudiantes.Add(estudianteNota);
            }

            return viewModel;
        }

        public async Task<bool> RegistrarNotaAsync(RegistrarNotaViewModel model, string profesorId)
        {
            var notaExistente = await _unitOfWork.Notas.GetNotaAsync(model.InscripcionId, model.ActividadId);

            if (notaExistente != null)
            {
                notaExistente.Calificacion = model.Calificacion;
                notaExistente.Observaciones = model.Observaciones;
                notaExistente.ModificadoPorId = profesorId;
                notaExistente.UltimaModificacion = DateTime.UtcNow;
                await _unitOfWork.Notas.UpdateAsync(notaExistente);
            }
            else
            {
                var nota = new Nota
                {
                    InscripcionId = model.InscripcionId,
                    ActividadId = model.ActividadId,
                    Calificacion = model.Calificacion,
                    Observaciones = model.Observaciones,
                    ModificadoPorId = profesorId
                };
                await _unitOfWork.Notas.AddAsync(nota);
            }

            await _unitOfWork.CompleteAsync();

            // Recalcular nota final
            await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(model.InscripcionId);

            return true;
        }

        public async Task<bool> ActualizarNotaAsync(int notaId, decimal calificacion, string profesorId)
        {
            var nota = await _unitOfWork.Notas.GetByIdAsync(notaId);
            if (nota == null) return false;

            nota.Calificacion = calificacion;
            nota.ModificadoPorId = profesorId;
            nota.UltimaModificacion = DateTime.UtcNow;

            await _unitOfWork.Notas.UpdateAsync(nota);
            await _unitOfWork.CompleteAsync();

            await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(nota.InscripcionId);

            return true;
        }

        public async Task<bool> EliminarNotaAsync(int notaId)
        {
            await _unitOfWork.Notas.DeleteAsync(notaId);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<decimal> CalcularNotaFinalAsync(int inscripcionId)
        {
            return await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(inscripcionId);
        }

        public async Task<ReporteNotasDTO> GenerarReporteNotasAsync(int materiaId)
        {
            var materia = await _unitOfWork.Materias.GetMateriaConDetallesAsync(materiaId);
            if (materia == null)
                throw new InvalidOperationException("Materia no encontrada");

            var actividades = await _unitOfWork.Actividades.GetActividadesPorMateriaAsync(materiaId);
            var inscripciones = await _unitOfWork.Inscripciones.GetInscripcionesPorMateriaAsync(materiaId);

            var reporte = new ReporteNotasDTO
            {
                MateriaNombre = materia.Nombre,
                ProfesorNombre = materia.Profesor?.NombreCompleto ?? "Sin asignar",
                ActividadesNombres = actividades.Select(a => a.Nombre).ToList()
            };

            foreach (var inscripcion in inscripciones)
            {
                // CORRECCIÓN: Usar el nuevo nombre del método
                var notasDict = await _unitOfWork.Notas.GetDiccionarioNotasPorActividadAsync(inscripcion.Id);
                var notaFinal = await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(inscripcion.Id);

                var estudianteNota = new EstudianteNotaDTO
                {
                    CI = inscripcion.Estudiante.CI,
                    NombreCompleto = inscripcion.Estudiante.NombreCompleto,
                    NotasPorActividad = new Dictionary<string, decimal?>(),
                    NotaFinal = notaFinal
                };

                foreach (var actividad in actividades)
                {
                    notasDict.TryGetValue(actividad.Id, out var nota);
                    estudianteNota.NotasPorActividad[actividad.Nombre] = nota;
                }

                reporte.Estudiantes.Add(estudianteNota);
            }

            return reporte;
        }

        public async Task<byte[]> ExportarExcelAsync(int materiaId)
        {
            var reporte = await GenerarReporteNotasAsync(materiaId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Notas");

            // Título
            worksheet.Cell(1, 1).Value = $"Reporte de Notas - {reporte.MateriaNombre}";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Range(1, 1, 1, reporte.ActividadesNombres.Count + 3).Merge();

            worksheet.Cell(2, 1).Value = $"Profesor: {reporte.ProfesorNombre}";
            worksheet.Cell(2, 1).Style.Font.FontSize = 12;

            // Encabezados
            worksheet.Cell(4, 1).Value = "CI";
            worksheet.Cell(4, 2).Value = "Nombre Completo";

            int col = 3;
            foreach (var actividad in reporte.ActividadesNombres)
            {
                worksheet.Cell(4, col).Value = actividad;
                col++;
            }
            worksheet.Cell(4, col).Value = "Nota Final";

            // Formato encabezados
            var headerRange = worksheet.Range(4, 1, 4, col);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 5;
            foreach (var estudiante in reporte.Estudiantes)
            {
                worksheet.Cell(row, 1).Value = estudiante.CI;
                worksheet.Cell(row, 2).Value = estudiante.NombreCompleto;

                col = 3;
                foreach (var actividadNombre in reporte.ActividadesNombres)
                {
                    if (estudiante.NotasPorActividad.TryGetValue(actividadNombre, out var nota))
                        worksheet.Cell(row, col).Value = nota;
                    col++;
                }
                worksheet.Cell(row, col).Value = estudiante.NotaFinal;
                row++;
            }

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}