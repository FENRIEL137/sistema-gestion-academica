using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Models.ViewModels;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Controllers.Profesor
{
    /// <summary>
    /// Controlador para funcionalidades del Profesor
    /// </summary>
    [Area("Profesor")]
    [Authorize(Roles = "Profesor")]
    public class ProfesorController : Controller
    {
        private readonly IProfesorService _profesorService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfesorController(
            IProfesorService profesorService,
            UserManager<ApplicationUser> userManager)
        {
            _profesorService = profesorService;
            _userManager = userManager;
        }

        // GET: /Profesor/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Obtener el ID del profesor desde el usuario logueado
            var user = await _userManager.GetUserAsync(User);
            var profesorId = 1; // Debes obtener esto de la base de datos

            var materias = await _profesorService.GetMateriasAsignadasAsync(profesorId);
            return View(materias);
        }

        // GET: /Profesor/Materias
        public async Task<IActionResult> Materias()
        {
            var profesorId = 1; // Obtener del usuario logueado
            var materias = await _profesorService.GetMateriasAsignadasAsync(profesorId);
            return View(materias);
        }

        // GET: /Profesor/Materia/5/Estudiantes
        public async Task<IActionResult> EstudiantesInscritos(int materiaId)
        {
            var estudiantes = await _profesorService.GetEstudiantesInscritosAsync(materiaId);
            ViewBag.MateriaId = materiaId;
            return View(estudiantes);
        }

        // GET: /Profesor/Actividades/5
        public async Task<IActionResult> Actividades(int materiaId)
        {
            var actividades = await _profesorService.GetActividadesAsync(materiaId);
            ViewBag.MateriaId = materiaId;
            return View(actividades);
        }

        // GET: /Profesor/CrearActividad/5
        public IActionResult CrearActividad(int materiaId)
        {
            var model = new ActividadViewModel { MateriaId = materiaId };
            return View(model);
        }

        // POST: /Profesor/CrearActividad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearActividad(ActividadViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _profesorService.CrearActividadAsync(model);
                    TempData["SuccessMessage"] = "Actividad creada exitosamente.";
                    return RedirectToAction(nameof(Actividades), new { materiaId = model.MateriaId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return View(model);
        }

        // GET: /Profesor/EditarActividad/5
        public async Task<IActionResult> EditarActividad(int id)
        {
            // Aquí deberías obtener la actividad por su ID
            var model = new ActividadViewModel { Id = id };
            return View(model);
        }

        // POST: /Profesor/EliminarActividad/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarActividad(int id, int materiaId)
        {
            await _profesorService.EliminarActividadAsync(id);
            TempData["SuccessMessage"] = "Actividad eliminada exitosamente.";
            return RedirectToAction(nameof(Actividades), new { materiaId });
        }

        // GET: /Profesor/Notas/5
        public async Task<IActionResult> Notas(int materiaId)
        {
            var model = await _profesorService.GetVistaNotasAsync(materiaId);
            return View(model);
        }

        // POST: /Profesor/RegistrarNota
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarNota(RegistrarNotaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                await _profesorService.RegistrarNotaAsync(model, userId!);
                TempData["SuccessMessage"] = "Nota registrada exitosamente.";
            }
            return RedirectToAction(nameof(Notas));
        }

        // POST: /Profesor/EliminarNota/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarNota(int id)
        {
            await _profesorService.EliminarNotaAsync(id);
            TempData["SuccessMessage"] = "Nota eliminada exitosamente.";
            return RedirectToAction(nameof(Notas));
        }

        // GET: /Profesor/ExportarExcel/5
        public async Task<IActionResult> ExportarExcel(int materiaId)
        {
            var excelBytes = await _profesorService.ExportarExcelAsync(materiaId);
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_Notas_Materia_{materiaId}.xlsx");
        }
    }
}