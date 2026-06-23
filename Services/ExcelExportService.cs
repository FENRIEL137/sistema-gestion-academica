using ClosedXML.Excel;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Services
{
    /// <summary>
    /// Servicio para generación de archivos Excel usando ClosedXML
    /// </summary>
    public class ExcelExportService : IExcelExportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExcelExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Exporta las notas de una materia específica
        /// </summary>
        public async Task<byte[]> ExportarNotasMateriaAsync(int materiaId)
        {
            var materia = await _unitOfWork.Materias.GetMateriaConDetallesAsync(materiaId);
            if (materia == null) throw new InvalidOperationException("Materia no encontrada");

            var actividades = await _unitOfWork.Actividades.GetActividadesPorMateriaAsync(materiaId);
            var inscripciones = await _unitOfWork.Inscripciones.GetInscripcionesPorMateriaAsync(materiaId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Notas");

            // Título
            worksheet.Cell(1, 1).Value = $"REPORTE DE NOTAS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, actividades.Count() + 4).Merge();
            worksheet.Range(1, 1, 1, actividades.Count() + 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Información de la materia
            worksheet.Cell(3, 1).Value = "Materia:";
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 2).Value = materia.Nombre;

            worksheet.Cell(4, 1).Value = "Profesor:";
            worksheet.Cell(4, 1).Style.Font.Bold = true;
            worksheet.Cell(4, 2).Value = materia.Profesor?.NombreCompleto ?? "Sin asignar";

            worksheet.Cell(5, 1).Value = "Fecha:";
            worksheet.Cell(5, 1).Style.Font.Bold = true;
            worksheet.Cell(5, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // Encabezados de la tabla
            int headerRow = 7;
            worksheet.Cell(headerRow, 1).Value = "N°";
            worksheet.Cell(headerRow, 2).Value = "CI";
            worksheet.Cell(headerRow, 3).Value = "Nombre Completo";

            int col = 4;
            foreach (var actividad in actividades)
            {
                worksheet.Cell(headerRow, col).Value = $"{actividad.Nombre}\n({actividad.ValorPorcentual}%)";
                worksheet.Cell(headerRow, col).Style.Alignment.WrapText = true;
                col++;
            }
            worksheet.Cell(headerRow, col).Value = "NOTA FINAL";
            worksheet.Cell(headerRow, col + 1).Value = "ESTADO";

            // Aplicar estilo a encabezados (CORREGIDO)
            var headerRange = worksheet.Range(headerRow, 1, headerRow, col + 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 11;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 123, 255);
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Datos de estudiantes
            int row = headerRow + 1;
            int numero = 1;

            foreach (var inscripcion in inscripciones)
            {
                var notas = await _unitOfWork.Notas.GetNotasPorInscripcionAsync(inscripcion.Id);
                var notaFinal = await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(inscripcion.Id);

                worksheet.Cell(row, 1).Value = numero++;
                worksheet.Cell(row, 2).Value = inscripcion.Estudiante.CI;
                worksheet.Cell(row, 3).Value = inscripcion.Estudiante.NombreCompleto;

                col = 4;
                foreach (var actividad in actividades)
                {
                    var nota = notas.FirstOrDefault(n => n.ActividadId == actividad.Id);
                    if (nota != null)
                    {
                        worksheet.Cell(row, col).Value = nota.Calificacion;

                        // Colorear según nota
                        if (nota.Calificacion >= 51)
                            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(212, 237, 218);
                        else
                            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                    }
                    else
                    {
                        worksheet.Cell(row, col).Value = "-";
                    }
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // Nota final
                worksheet.Cell(row, col).Value = notaFinal;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Colorear nota final
                if (notaFinal >= 51)
                {
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(40, 167, 69);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                }
                else
                {
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(220, 53, 69);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                }

                // Estado
                worksheet.Cell(row, col + 1).Value = notaFinal >= 51 ? "APROBADO" : "REPROBADO";
                worksheet.Cell(row, col + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col + 1).Style.Font.Bold = true;

                // Bordes para toda la fila
                var dataRange = worksheet.Range(row, 1, row, col + 1);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            // Estadísticas
            row += 2;
            worksheet.Cell(row, 1).Value = "ESTADÍSTICAS";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 14;
            row++;

            var aprobados = inscripciones.Count(i => (i.NotaFinal ?? 0) >= 51);
            var reprobados = inscripciones.Count() - aprobados;
            var promedio = inscripciones.Any() ? inscripciones.Average(i => i.NotaFinal ?? 0) : 0;

            worksheet.Cell(row, 1).Value = "Total Estudiantes:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = inscripciones.Count();
            row++;

            worksheet.Cell(row, 1).Value = "Aprobados:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = aprobados;
            worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.Green;
            row++;

            worksheet.Cell(row, 1).Value = "Reprobados:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reprobados;
            worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.Red;
            row++;

            worksheet.Cell(row, 1).Value = "Promedio General:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = Math.Round(promedio, 2);

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();
            // Ancho mínimo para columnas
            for (int i = 1; i <= col + 1; i++)
            {
                if (worksheet.Column(i).Width < 10)
                    worksheet.Column(i).Width = 10;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Exporta listado completo de estudiantes
        /// </summary>
        public async Task<byte[]> ExportarListadoEstudiantesAsync()
        {
            var estudiantes = await _unitOfWork.Estudiantes.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Estudiantes");

            // Título
            worksheet.Cell(1, 1).Value = "LISTADO DE ESTUDIANTES";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(2, 1).Value = $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";

            // Encabezados
            var headers = new[] { "N°", "CI", "Nombre Completo", "Teléfono", "Correo", "Fecha Inscripción", "Estado" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 123, 255);
                worksheet.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 5;
            int num = 1;
            foreach (var estudiante in estudiantes)
            {
                worksheet.Cell(row, 1).Value = num++;
                worksheet.Cell(row, 2).Value = estudiante.CI;
                worksheet.Cell(row, 3).Value = estudiante.NombreCompleto;
                worksheet.Cell(row, 4).Value = estudiante.Telefono ?? "";
                worksheet.Cell(row, 5).Value = estudiante.Correo ?? "";
                worksheet.Cell(row, 6).Value = estudiante.FechaInscripcion.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 7).Value = estudiante.Activo ? "Activo" : "Inactivo";

                // Color según estado
                worksheet.Cell(row, 7).Style.Font.FontColor = estudiante.Activo ? XLColor.Green : XLColor.Red;

                row++;
            }

            // Bordes para la tabla
            var tableRange = worksheet.Range(4, 1, row - 1, 7);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Exporta listado de profesores
        /// </summary>
        public async Task<byte[]> ExportarListadoProfesoresAsync()
        {
            var profesores = await _unitOfWork.Profesores.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Profesores");

            // Título
            worksheet.Cell(1, 1).Value = "LISTADO DE PROFESORES";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Encabezados
            var headers = new[] { "N°", "CI", "Nombre Completo", "Especialidad", "Teléfono", "Correo", "Estado" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(3, i + 1).Value = headers[i];
                worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(40, 167, 69);
                worksheet.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(3, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 4;
            int num = 1;
            foreach (var profesor in profesores)
            {
                worksheet.Cell(row, 1).Value = num++;
                worksheet.Cell(row, 2).Value = profesor.CI;
                worksheet.Cell(row, 3).Value = profesor.NombreCompleto;
                worksheet.Cell(row, 4).Value = profesor.Especialidad;
                worksheet.Cell(row, 5).Value = profesor.Telefono ?? "";
                worksheet.Cell(row, 6).Value = profesor.Correo ?? "";
                worksheet.Cell(row, 7).Value = profesor.Activo ? "Activo" : "Inactivo";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Exporta reporte de pagos en un rango de fechas
        /// </summary>
        public async Task<byte[]> ExportarReportePagosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddMonths(-1);
            var fin = fechaFin ?? DateTime.Now;
            var pagos = await _unitOfWork.Pagos.GetPagosPorPeriodoAsync(inicio, fin);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pagos");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE PAGOS";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(2, 1).Value = $"Período: {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}";

            // Encabezados
            var headers = new[] { "N°", "Estudiante", "CI", "Concepto", "Monto", "Fecha", "Estado" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 193, 7);
                worksheet.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 5;
            int num = 1;
            decimal total = 0;
            foreach (var pago in pagos)
            {
                worksheet.Cell(row, 1).Value = num++;
                worksheet.Cell(row, 2).Value = pago.Estudiante?.NombreCompleto ?? "N/A";
                worksheet.Cell(row, 3).Value = pago.Estudiante?.CI ?? "N/A";
                worksheet.Cell(row, 4).Value = pago.Concepto ?? pago.Tipo.ToString();
                worksheet.Cell(row, 5).Value = pago.Monto;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$ #,##0.00";
                worksheet.Cell(row, 6).Value = pago.FechaPago.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 7).Value = pago.Estado.ToString();

                total += pago.Monto;
                row++;
            }

            // Total
            row++;
            worksheet.Cell(row, 4).Value = "TOTAL:";
            worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 5).Value = total;
            worksheet.Cell(row, 5).Style.Font.Bold = true;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$ #,##0.00";

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Exporta listado de estudiantes con deudas
        /// </summary>
        public async Task<byte[]> ExportarEstudiantesConDeudaAsync()
        {
            var estudiantesConDeuda = await _unitOfWork.Estudiantes.GetEstudiantesConDeudasAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Estudiantes con Deuda");

            // Título
            worksheet.Cell(1, 1).Value = "ESTUDIANTES CON DEUDAS PENDIENTES";
            worksheet.Range(1, 1, 1, 6).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.Red;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(2, 1).Value = $"Fecha: {DateTime.Now:dd/MM/yyyy}";

            // Encabezados
            var headers = new[] { "N°", "CI", "Nombre Completo", "Materias Pendientes", "Deuda Total", "Teléfono" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(220, 53, 69);
                worksheet.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 5;
            int num = 1;
            foreach (var estudiante in estudiantesConDeuda)
            {
                var deuda = await _unitOfWork.Estudiantes.GetDeudaTotalAsync(estudiante.Id);
                var materiasPendientes = estudiante.Inscripciones
                    .Count(i => !i.PagoRealizado && i.Estado == Models.Entities.EstadoInscripcion.Activa);

                worksheet.Cell(row, 1).Value = num++;
                worksheet.Cell(row, 2).Value = estudiante.CI;
                worksheet.Cell(row, 3).Value = estudiante.NombreCompleto;
                worksheet.Cell(row, 4).Value = materiasPendientes;
                worksheet.Cell(row, 5).Value = deuda;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$ #,##0.00";
                worksheet.Cell(row, 6).Value = estudiante.Telefono ?? "";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Exporta estado académico de un estudiante
        /// </summary>
        public async Task<byte[]> ExportarEstadoAcademicoEstudianteAsync(int estudianteId)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(estudianteId);
            if (estudiante == null) throw new InvalidOperationException("Estudiante no encontrado");

            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(estudianteId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Estado Académico");

            // Información del estudiante
            worksheet.Cell(1, 1).Value = "ESTADO ACADÉMICO";
            worksheet.Range(1, 1, 1, 6).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3, 1).Value = "Estudiante:";
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 2).Value = estudiante.NombreCompleto;

            worksheet.Cell(4, 1).Value = "CI:";
            worksheet.Cell(4, 1).Style.Font.Bold = true;
            worksheet.Cell(4, 2).Value = estudiante.CI;

            worksheet.Cell(5, 1).Value = "Fecha:";
            worksheet.Cell(5, 1).Style.Font.Bold = true;
            worksheet.Cell(5, 2).Value = DateTime.Now.ToString("dd/MM/yyyy");

            // Encabezados
            var headers = new[] { "N°", "Materia", "Profesor", "Horario", "Nota Final", "Estado" };
            int headerRow = 7;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(headerRow, i + 1).Value = headers[i];
                worksheet.Cell(headerRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 123, 255);
                worksheet.Cell(headerRow, i + 1).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(headerRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 8;
            int num = 1;
            foreach (var inscripcion in inscripciones)
            {
                var notaFinal = await _unitOfWork.Inscripciones.CalcularNotaFinalAsync(inscripcion.Id);

                worksheet.Cell(row, 1).Value = num++;
                worksheet.Cell(row, 2).Value = inscripcion.Materia.Nombre;
                worksheet.Cell(row, 3).Value = inscripcion.Materia.Profesor?.NombreCompleto ?? "Sin asignar";
                worksheet.Cell(row, 4).Value = inscripcion.Materia.Horario?.HorarioCompleto ?? "Sin horario";
                worksheet.Cell(row, 5).Value = notaFinal;
                worksheet.Cell(row, 6).Value = notaFinal >= 51 ? "APROBADO" : "REPROBADO";

                // Colorear según aprobación
                if (notaFinal >= 51)
                {
                    worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(212, 237, 218);
                    worksheet.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(40, 167, 69);
                    worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.White;
                }
                else
                {
                    worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                    worksheet.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(220, 53, 69);
                    worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.White;
                }

                row++;
            }

            // Resumen
            row += 2;
            var aprobadas = inscripciones.Count(i => (i.NotaFinal ?? 0) >= 51);
            var promedio = inscripciones.Any() ? inscripciones.Average(i => i.NotaFinal ?? 0) : 0;

            worksheet.Cell(row, 1).Value = "RESUMEN";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            worksheet.Cell(row, 1).Value = "Total Materias:";
            worksheet.Cell(row, 2).Value = inscripciones.Count();
            row++;

            worksheet.Cell(row, 1).Value = "Aprobadas:";
            worksheet.Cell(row, 2).Value = aprobadas;
            row++;

            worksheet.Cell(row, 1).Value = "Reprobadas:";
            worksheet.Cell(row, 2).Value = inscripciones.Count() - aprobadas;
            row++;

            worksheet.Cell(row, 1).Value = "Promedio General:";
            worksheet.Cell(row, 2).Value = Math.Round(promedio, 2);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}