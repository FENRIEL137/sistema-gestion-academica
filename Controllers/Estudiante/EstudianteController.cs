using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Controllers.Estudiante
{
    /// <summary>
    /// Controlador para funcionalidades del Estudiante
    /// </summary>
    [Area("Estudiante")]
    [Authorize(Roles = "Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly IEstudianteService _estudianteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EstudianteController(
            IEstudianteService estudianteService,
            UserManager<ApplicationUser> userManager)
        {
            _estudianteService = estudianteService;
            _userManager = userManager;
        }

        private async Task<int> GetEstudianteId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return 0;

            // Aquí necesitarías obtener el Id del estudiante desde el UserId
            // Por ahora retornamos un valor de ejemplo
            return 1;
        }

        // GET: /Estudiante/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var estudianteId = await GetEstudianteId();
            var estado = await _estudianteService.GetEstadoAcademicoAsync(estudianteId);
            var deuda = await _estudianteService.GetDeudaTotalAsync(estudianteId);

            ViewBag.DeudaTotal = deuda;
            return View(estado);
        }

        // ============ INSCRIPCIONES ============

        // GET: /Estudiante/Inscripciones
        public async Task<IActionResult> Inscripciones()
        {
            var estudianteId = await GetEstudianteId();
            var materiasDisponibles = await _estudianteService.GetMateriasDisponiblesAsync();
            var materiasInscritas = await _estudianteService.GetMateriasInscritasAsync(estudianteId);

            ViewBag.MateriasDisponibles = materiasDisponibles;
            return View(materiasInscritas);
        }

        // POST: /Estudiante/Inscribir/5
        [HttpPost]
        
        public async Task<IActionResult> Inscribir(int materiaId)
        {
            try
            {
                var estudianteId = await GetEstudianteId();
                await _estudianteService.InscribirMateriaAsync(estudianteId, materiaId);
                TempData["SuccessMessage"] = "Inscripción realizada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Inscripciones));
        }

        // GET: /Estudiante/Horario
        public async Task<IActionResult> Horario()
        {
            var estudianteId = await GetEstudianteId();
            var horario = await _estudianteService.GetHorarioEstudianteAsync(estudianteId);
            return View(horario);
        }

        // ============ PAGOS ============

        // GET: /Estudiante/Pagos
        public async Task<IActionResult> Pagos()
        {
            var estudianteId = await GetEstudianteId();
            var historial = await _estudianteService.GetHistorialPagosAsync(estudianteId);
            var deudas = await _estudianteService.GetDeudasPendientesAsync(estudianteId);
            var deudaTotal = await _estudianteService.GetDeudaTotalAsync(estudianteId);

            ViewBag.Historial = historial;
            ViewBag.Deudas = deudas;
            ViewBag.DeudaTotal = deudaTotal;

            return View();
        }

        // POST: /Estudiante/Pagar
        [HttpPost]
        
        public async Task<IActionResult> Pagar(int inscripcionId, decimal monto)
        {
            try
            {
                var estudianteId = await GetEstudianteId();
                await _estudianteService.RealizarPagoAsync(estudianteId, inscripcionId, monto);
                TempData["SuccessMessage"] = "Pago realizado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Pagos));
        }

        // ============ CONSULTAS ============

        // GET: /Estudiante/Notas
        public async Task<IActionResult> Notas()
        {
            var estudianteId = await GetEstudianteId();
            var notas = await _estudianteService.GetNotasAsync(estudianteId);
            return View(notas);
        }

        // GET: /Estudiante/EstadoAcademico
        public async Task<IActionResult> EstadoAcademico()
        {
            var estudianteId = await GetEstudianteId();
            var estado = await _estudianteService.GetEstadoAcademicoAsync(estudianteId);
            return View(estado);
        }
    }
}