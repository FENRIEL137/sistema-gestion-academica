using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

// Alias para evitar conflictos
using EstudianteEntity = SistemaGestionAcademica.Models.Entities.Estudiante;

namespace SistemaGestionAcademica.Controllers.Empleado
{
    /// <summary>
    /// Controlador para funcionalidades del Empleado
    /// </summary>
    [Area("Empleado")]
    [Authorize(Roles = "Empleado")]
    public class EmpleadoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmpleadoController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: /Empleado/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalEstudiantes = await _unitOfWork.Estudiantes.CountAsync(e => e.Activo);
            ViewBag.EstudiantesConDeuda = (await _unitOfWork.Estudiantes.GetEstudiantesConDeudasAsync()).Count();
            ViewBag.InscripcionesPendientes = (await _unitOfWork.Inscripciones.GetInscripcionesPendientesPagoAsync()).Count();

            return View();
        }

        // GET: /Empleado/Estudiantes
        public async Task<IActionResult> Estudiantes()
        {
            var estudiantes = await _unitOfWork.Estudiantes.GetAllAsync();
            return View(estudiantes);
        }

        // GET: /Empleado/Estudiantes/Create
        public IActionResult CrearEstudiante()
        {
            return View(new EstudianteEntity());
        }

        // POST: /Empleado/Estudiantes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEstudiante(EstudianteEntity estudiante, string email, string password)
        {
            if (ModelState.IsValid)
            {
                if (await _unitOfWork.Estudiantes.GetByCIAsync(estudiante.CI) != null)
                {
                    ModelState.AddModelError("CI", "Ya existe un estudiante con esta cédula.");
                    return View(estudiante);
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NombreCompleto = $"{estudiante.Nombre} {estudiante.Apellido}",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Estudiante");
                    estudiante.UserId = user.Id;
                    estudiante.FechaInscripcion = DateTime.UtcNow;
                    estudiante.Activo = true;

                    await _unitOfWork.Estudiantes.AddAsync(estudiante);
                    await _unitOfWork.CompleteAsync();

                    TempData["SuccessMessage"] = "Estudiante registrado exitosamente.";
                    return RedirectToAction(nameof(Estudiantes));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(estudiante);
        }

        // GET: /Empleado/Estudiantes/Details/5
        public async Task<IActionResult> DetallesEstudiante(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            if (estudiante == null) return NotFound();

            ViewBag.Inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(id);
            ViewBag.HistorialPagos = await _unitOfWork.Estudiantes.GetHistorialPagosAsync(id);
            ViewBag.DeudaTotal = await _unitOfWork.Estudiantes.GetDeudaTotalAsync(id);

            return View(estudiante);
        }

        // GET: /Empleado/Estudiantes/Edit/5
        public async Task<IActionResult> EditarEstudiante(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            if (estudiante == null) return NotFound();
            return View(estudiante);
        }

        // POST: /Empleado/Estudiantes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEstudiante(int id, EstudianteEntity estudiante)
        {
            if (id != estudiante.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingEstudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
                if (existingEstudiante == null) return NotFound();

                existingEstudiante.Nombre = estudiante.Nombre;
                existingEstudiante.Apellido = estudiante.Apellido;
                existingEstudiante.Telefono = estudiante.Telefono;
                existingEstudiante.Correo = estudiante.Correo;
                existingEstudiante.PagoInicial = estudiante.PagoInicial;

                await _unitOfWork.Estudiantes.UpdateAsync(existingEstudiante);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Estudiante actualizado exitosamente.";
                return RedirectToAction(nameof(Estudiantes));
            }
            return View(estudiante);
        }

        // POST: /Empleado/Estudiantes/Baja/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DarBajaEstudiante(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            if (estudiante == null) return NotFound();

            estudiante.Activo = false;
            estudiante.FechaBaja = DateTime.UtcNow;
            await _unitOfWork.Estudiantes.UpdateAsync(estudiante);

            var inscripciones = await _unitOfWork.Estudiantes.GetInscripcionesEstudianteAsync(id);
            foreach (var inscripcion in inscripciones.Where(i => i.Estado == EstadoInscripcion.Activa))
            {
                inscripcion.Estado = EstadoInscripcion.BajaDefinitiva;
                inscripcion.FechaBaja = DateTime.UtcNow;
                await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            }

            await _unitOfWork.CompleteAsync();
            TempData["SuccessMessage"] = "Estudiante dado de baja exitosamente.";
            return RedirectToAction(nameof(Estudiantes));
        }

        // POST: /Empleado/Estudiantes/Reactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivarEstudiante(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            if (estudiante == null) return NotFound();

            estudiante.Activo = true;
            estudiante.FechaBaja = null;
            await _unitOfWork.Estudiantes.UpdateAsync(estudiante);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Estudiante reactivado exitosamente.";
            return RedirectToAction(nameof(Estudiantes));
        }

        // GET: /Empleado/Pagos
        public async Task<IActionResult> Pagos()
        {
            var pagosPendientes = await _unitOfWork.Pagos.GetPagosPendientesAsync();
            return View(pagosPendientes);
        }

        // POST: /Empleado/RegistrarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(int inscripcionId, decimal monto, string concepto)
        {
            try
            {
                var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(inscripcionId);
                if (inscripcion == null)
                {
                    TempData["ErrorMessage"] = "Inscripcion no encontrada.";
                    return RedirectToAction(nameof(Pagos));
                }

                var userId = _userManager.GetUserId(User);

                var pago = new Pago
                {
                    InscripcionId = inscripcionId,
                    EstudianteId = inscripcion.EstudianteId,
                    Monto = monto,
                    FechaPago = DateTime.UtcNow,
                    Tipo = TipoPago.Materia,
                    Concepto = concepto ?? "Pago de materia",
                    RegistradoPorId = userId,
                    Estado = EstadoPago.Completado
                };

                await _unitOfWork.Pagos.AddAsync(pago);

                // Verificar si el pago cubre el costo total
                var pagosRealizados = await _unitOfWork.Pagos.GetPagosPorInscripcionAsync(inscripcionId);
                var totalPagado = pagosRealizados.Sum(p => p.Monto);

                if (totalPagado >= inscripcion.Materia.Costo)
                {
                    inscripcion.PagoRealizado = true;
                    await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
                }

                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Pago registrado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al registrar pago: {ex.Message}";
            }

            return RedirectToAction(nameof(Pagos));
        }

        // GET: /Empleado/Deudas
        public async Task<IActionResult> Deudas()
        {
            var inscripcionesPendientes = await _unitOfWork.Inscripciones.GetInscripcionesPendientesPagoAsync();

            var config = await _unitOfWork.Configuraciones.GetConfiguracionActualAsync();
            var hoy = DateTime.UtcNow;
            var inicioPeriodo = new DateTime(hoy.Year, hoy.Month, config?.DiaInicioPagos ?? 23);
            var finPeriodo = new DateTime(hoy.Year, hoy.Month, config?.DiaFinPagos ?? 30);

            ViewBag.PeriodoPago = hoy >= inicioPeriodo && hoy <= finPeriodo;
            ViewBag.InicioPeriodo = inicioPeriodo;
            ViewBag.FinPeriodo = finPeriodo;

            return View(inscripcionesPendientes);
        }

        // POST: /Empleado/RegistrarInscripcion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarInscripcion(int estudianteId, int materiaId)
        {
            var inscripcion = new Inscripcion
            {
                EstudianteId = estudianteId,
                MateriaId = materiaId,
                FechaInscripcion = DateTime.UtcNow,
                Estado = EstadoInscripcion.Activa,
                PagoRealizado = false
            };

            await _unitOfWork.Inscripciones.AddAsync(inscripcion);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Inscripción registrada exitosamente.";
            return RedirectToAction(nameof(Estudiantes));
        }

        // POST: /Empleado/BajaInscripcion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BajaInscripcion(int id)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(id);
            if (inscripcion == null) return NotFound();

            inscripcion.Estado = EstadoInscripcion.BajaTemporal;
            inscripcion.FechaBaja = DateTime.UtcNow;
            await _unitOfWork.Inscripciones.UpdateAsync(inscripcion);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Inscripción dada de baja.";
            return RedirectToAction(nameof(Estudiantes));
        }
    }
}