using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;

// Alias para evitar conflictos de namespace con los controladores
using ProfesorEntity = SistemaGestionAcademica.Models.Entities.Profesor;
using EmpleadoEntity = SistemaGestionAcademica.Models.Entities.Empleado;
using EstudianteEntity = SistemaGestionAcademica.Models.Entities.Estudiante;
using MateriaEntity = SistemaGestionAcademica.Models.Entities.Materia;
using AulaEntity = SistemaGestionAcademica.Models.Entities.Aula;

namespace SistemaGestionAcademica.Controllers.Admin
{
    /// <summary>
    /// Controlador para funcionalidades del Administrador
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AdminController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalEstudiantes = await _unitOfWork.Estudiantes.CountAsync(e => e.Activo);
            ViewBag.TotalProfesores = await _unitOfWork.Profesores.CountAsync(p => p.Activo);
            ViewBag.TotalEmpleados = await _unitOfWork.Empleados.CountAsync(e => e.Activo);
            ViewBag.TotalMaterias = await _unitOfWork.Materias.CountAsync(m => m.Activo);
            ViewBag.TotalAulas = await _unitOfWork.Aulas.CountAsync(a => a.Activo);
            ViewBag.EstudiantesConDeuda = await _unitOfWork.Estudiantes.GetEstudiantesConDeudasAsync();
            ViewBag.IngresosTotales = await _unitOfWork.Pagos.GetAllAsync();

            return View();
        }

        // ============ GESTIÓN DE PROFESORES ============

        // GET: /Admin/Profesores
        public async Task<IActionResult> Profesores()
        {
            var profesores = await _unitOfWork.Profesores.GetAllAsync();
            return View(profesores);
        }

        // GET: /Admin/Profesores/Create
        public IActionResult CrearProfesor()
        {
            return View(new ProfesorEntity());
        }

        // POST: /Admin/Profesores/Create
        [HttpPost]
        public async Task<IActionResult> CrearProfesor(ProfesorEntity profesor, string email, string password)
        {
            if (ModelState.IsValid)
            {
                // Verificar CI único
                if (await _unitOfWork.Profesores.GetByCIAsync(profesor.CI) != null)
                {
                    ModelState.AddModelError("CI", "Ya existe un profesor con esta cédula.");
                    return View(profesor);
                }

                // Crear usuario de Identity
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NombreCompleto = $"{profesor.Nombre} {profesor.Apellido}",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Profesor");
                    profesor.UserId = user.Id;
                    profesor.FechaContratacion = DateTime.UtcNow;
                    profesor.Activo = true;

                    await _unitOfWork.Profesores.AddAsync(profesor);
                    await _unitOfWork.CompleteAsync();

                    TempData["SuccessMessage"] = "Profesor creado exitosamente.";
                    return RedirectToAction(nameof(Profesores));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(profesor);
        }

        // GET: /Admin/Profesores/Edit/5
        public async Task<IActionResult> EditarProfesor(int id)
        {
            var profesor = await _unitOfWork.Profesores.GetByIdAsync(id);
            if (profesor == null) return NotFound();
            return View(profesor);
        }

        // POST: /Admin/Profesores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProfesor(int id, ProfesorEntity profesor)
        {
            if (id != profesor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProfesor = await _unitOfWork.Profesores.GetByIdAsync(id);
                if (existingProfesor == null) return NotFound();

                existingProfesor.Nombre = profesor.Nombre;
                existingProfesor.Apellido = profesor.Apellido;
                existingProfesor.Telefono = profesor.Telefono;
                existingProfesor.Especialidad = profesor.Especialidad;
                existingProfesor.Correo = profesor.Correo;

                await _unitOfWork.Profesores.UpdateAsync(existingProfesor);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Profesor actualizado exitosamente.";
                return RedirectToAction(nameof(Profesores));
            }
            return View(profesor);
        }

        // POST: /Admin/Profesores/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProfesor(int id)
        {
            var profesor = await _unitOfWork.Profesores.GetByIdAsync(id);
            if (profesor == null) return NotFound();

            profesor.Activo = false;
            profesor.FechaBaja = DateTime.UtcNow;
            await _unitOfWork.Profesores.UpdateAsync(profesor);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Profesor dado de baja exitosamente.";
            return RedirectToAction(nameof(Profesores));
        }

        // POST: /Admin/Profesores/Contratar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContratarProfesor(int id)
        {
            var profesor = await _unitOfWork.Profesores.GetByIdAsync(id);
            if (profesor == null) return NotFound();

            profesor.Activo = true;
            profesor.FechaBaja = null;
            profesor.FechaContratacion = DateTime.UtcNow;
            await _unitOfWork.Profesores.UpdateAsync(profesor);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Profesor contratado exitosamente.";
            return RedirectToAction(nameof(Profesores));
        }

        // ============ GESTIÓN DE EMPLEADOS ============

        // GET: /Admin/Empleados
        public async Task<IActionResult> Empleados()
        {
            var empleados = await _unitOfWork.Empleados.GetAllAsync();
            return View(empleados);
        }

        // GET: /Admin/Empleados/Create
        public IActionResult CrearEmpleado()
        {
            return View(new EmpleadoEntity());
        }

        // POST: /Admin/Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEmpleado(EmpleadoEntity empleado, string email, string password)
        {
            if (ModelState.IsValid)
            {
                if (await _unitOfWork.Empleados.GetByCIAsync(empleado.CI) != null)
                {
                    ModelState.AddModelError("CI", "Ya existe un empleado con esta cédula.");
                    return View(empleado);
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NombreCompleto = $"{empleado.Nombre} {empleado.Apellido}",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Empleado");
                    empleado.UserId = user.Id;
                    empleado.FechaContratacion = DateTime.UtcNow;
                    empleado.Activo = true;

                    await _unitOfWork.Empleados.AddAsync(empleado);
                    await _unitOfWork.CompleteAsync();

                    TempData["SuccessMessage"] = "Empleado creado exitosamente.";
                    return RedirectToAction(nameof(Empleados));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(empleado);
        }

        // GET: /Admin/Empleados/Edit/5
        public async Task<IActionResult> EditarEmpleado(int id)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(id);
            if (empleado == null) return NotFound();
            return View(empleado);
        }

        // POST: /Admin/Empleados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEmpleado(int id, EmpleadoEntity empleado)
        {
            if (id != empleado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingEmpleado = await _unitOfWork.Empleados.GetByIdAsync(id);
                if (existingEmpleado == null) return NotFound();

                existingEmpleado.Nombre = empleado.Nombre;
                existingEmpleado.Apellido = empleado.Apellido;
                existingEmpleado.Telefono = empleado.Telefono;
                existingEmpleado.Cargo = empleado.Cargo;
                existingEmpleado.Correo = empleado.Correo;

                await _unitOfWork.Empleados.UpdateAsync(existingEmpleado);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Empleado actualizado exitosamente.";
                return RedirectToAction(nameof(Empleados));
            }
            return View(empleado);
        }

        // POST: /Admin/Empleados/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(id);
            if (empleado == null) return NotFound();

            empleado.Activo = false;
            empleado.FechaBaja = DateTime.UtcNow;
            await _unitOfWork.Empleados.UpdateAsync(empleado);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Empleado dado de baja exitosamente.";
            return RedirectToAction(nameof(Empleados));
        }

        // ============ GESTIÓN DE MATERIAS ============

        // GET: /Admin/Materias
        public async Task<IActionResult> Materias()
        {
            var materias = await _unitOfWork.Materias.GetMateriasDisponiblesAsync();
            return View(materias);
        }

        // GET: /Admin/Materias/Create
        public async Task<IActionResult> CrearMateria()
        {
            ViewBag.Profesores = new SelectList(
                await _unitOfWork.Profesores.GetProfesoresActivosAsync(), "Id", "NombreCompleto");
            ViewBag.Aulas = new SelectList(
                await _unitOfWork.Aulas.GetAulasDisponiblesAsync(), "Id", "Nombre");
            ViewBag.Horarios = new SelectList(
                await _unitOfWork.Horarios.GetHorariosDisponiblesAsync(), "Id", "HorarioCompleto");

            return View(new MateriaEntity());
        }

        // POST: /Admin/Materias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMateria(MateriaEntity materia)
        {
            if (ModelState.IsValid)
            {
                materia.Activo = true;
                await _unitOfWork.Materias.AddAsync(materia);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Materia creada exitosamente.";
                return RedirectToAction(nameof(Materias));
            }

            ViewBag.Profesores = new SelectList(
                await _unitOfWork.Profesores.GetProfesoresActivosAsync(), "Id", "NombreCompleto", materia.ProfesorId);
            ViewBag.Aulas = new SelectList(
                await _unitOfWork.Aulas.GetAulasDisponiblesAsync(), "Id", "Nombre", materia.AulaId);
            ViewBag.Horarios = new SelectList(
                await _unitOfWork.Horarios.GetHorariosDisponiblesAsync(), "Id", "HorarioCompleto", materia.HorarioId);

            return View(materia);
        }

        // GET: /Admin/Materias/Edit/5
        public async Task<IActionResult> EditarMateria(int id)
        {
            var materia = await _unitOfWork.Materias.GetByIdAsync(id);
            if (materia == null) return NotFound();

            ViewBag.Profesores = new SelectList(
                await _unitOfWork.Profesores.GetProfesoresActivosAsync(), "Id", "NombreCompleto", materia.ProfesorId);
            ViewBag.Aulas = new SelectList(
                await _unitOfWork.Aulas.GetAulasDisponiblesAsync(), "Id", "Nombre", materia.AulaId);
            ViewBag.Horarios = new SelectList(
                await _unitOfWork.Horarios.GetHorariosDisponiblesAsync(), "Id", "HorarioCompleto", materia.HorarioId);

            return View(materia);
        }

        // POST: /Admin/Materias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMateria(int id, MateriaEntity materia)
        {
            if (id != materia.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _unitOfWork.Materias.UpdateAsync(materia);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Materia actualizada exitosamente.";
                return RedirectToAction(nameof(Materias));
            }

            ViewBag.Profesores = new SelectList(
                await _unitOfWork.Profesores.GetProfesoresActivosAsync(), "Id", "NombreCompleto", materia.ProfesorId);
            ViewBag.Aulas = new SelectList(
                await _unitOfWork.Aulas.GetAulasDisponiblesAsync(), "Id", "Nombre", materia.AulaId);
            ViewBag.Horarios = new SelectList(
                await _unitOfWork.Horarios.GetHorariosDisponiblesAsync(), "Id", "HorarioCompleto", materia.HorarioId);

            return View(materia);
        }

        // ============ GESTIÓN DE AULAS ============

        // GET: /Admin/Aulas
        public async Task<IActionResult> Aulas()
        {
            var aulas = await _unitOfWork.Aulas.GetAllAsync();
            return View(aulas);
        }

        // GET: /Admin/Aulas/Create
        public IActionResult CrearAula()
        {
            return View(new AulaEntity());
        }

        // POST: /Admin/Aulas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAula(AulaEntity aula)
        {
            if (ModelState.IsValid)
            {
                aula.Activo = true;
                await _unitOfWork.Aulas.AddAsync(aula);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Aula creada exitosamente.";
                return RedirectToAction(nameof(Aulas));
            }
            return View(aula);
        }

        // GET: /Admin/EditarAula/5
        public async Task<IActionResult> EditarAula(int id)
        {
            var aula = await _unitOfWork.Aulas.GetByIdAsync(id);
            if (aula == null) return NotFound();
            return View(aula);
        }

        // POST: /Admin/EditarAula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAula(int id, AulaEntity aula)
        {
            if (id != aula.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _unitOfWork.Aulas.UpdateAsync(aula);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Aula actualizada exitosamente.";
                return RedirectToAction(nameof(Aulas));
            }
            return View(aula);
        }

        // ============ CONFIGURACIÓN INSTITUCIONAL ============

        // GET: /Admin/Configuracion
        public async Task<IActionResult> Configuracion()
        {
            var config = await _unitOfWork.Configuraciones.GetConfiguracionActualAsync();
            if (config == null)
            {
                config = new ConfiguracionInstitucional
                {
                    NombreInstitucion = "Mi Institución",
                    PagoInicialInscripcion = 500.00m,
                    CostoBaseMateria = 300.00m,
                    PorcentajePenalizacionMora = 5.00m,
                    DiaInicioPagos = 23,
                    DiaFinPagos = 30,
                    PeriodoActual = "2026-I"
                };
            }
            return View(config);
        }

        // POST: /Admin/Configuracion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Configuracion(ConfiguracionInstitucional config)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                await _unitOfWork.Configuraciones.ActualizarConfiguracionAsync(config, userId!);
                TempData["SuccessMessage"] = "Configuración actualizada exitosamente.";
                return RedirectToAction(nameof(Dashboard));
            }
            return View(config);
        }

        // ============ CONSULTAS GLOBALES ============

        // GET: /Admin/Consultas
        public async Task<IActionResult> Consultas()
        {
            ViewBag.Estudiantes = await _unitOfWork.Estudiantes.GetAllAsync();
            ViewBag.Profesores = await _unitOfWork.Profesores.GetAllAsync();
            ViewBag.Empleados = await _unitOfWork.Empleados.GetAllAsync();
            ViewBag.Materias = await _unitOfWork.Materias.GetMateriasDisponiblesAsync();
            ViewBag.Aulas = await _unitOfWork.Aulas.GetAulasDisponiblesAsync();

            return View();
        }
    }
}