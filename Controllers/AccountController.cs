using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Models.ViewModels;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Controllers
{
    /// <summary>
    /// Controlador para gestión de autenticación y cuentas de usuario
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // =============================================
            // FORZAR CREACION DE ROLES Y ADMIN
            // =============================================
            if (!await _roleManager.RoleExistsAsync("Administrador"))
                await _roleManager.CreateAsync(new ApplicationRole("Administrador"));

            if (!await _roleManager.RoleExistsAsync("Empleado"))
                await _roleManager.CreateAsync(new ApplicationRole("Empleado"));

            if (!await _roleManager.RoleExistsAsync("Profesor"))
                await _roleManager.CreateAsync(new ApplicationRole("Profesor"));

            if (!await _roleManager.RoleExistsAsync("Estudiante"))
                await _roleManager.CreateAsync(new ApplicationRole("Estudiante"));

            var adminUser = await _userManager.FindByEmailAsync("admin@sistema.edu");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@sistema.edu",
                    Email = "admin@sistema.edu",
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true,
                    Activo = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                await _userManager.CreateAsync(adminUser, "Admin123!");
            }

            if (!await _userManager.IsInRoleAsync(adminUser, "Administrador"))
                await _userManager.AddToRoleAsync(adminUser, "Administrador");
            // =============================================

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Correo o contrasena incorrectos.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                user.UltimoAcceso = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation($"LOGIN OK: {user.Email} - Roles: {string.Join(", ", await _userManager.GetRolesAsync(user))}");

                if (await _userManager.IsInRoleAsync(user, "Administrador"))
                    return RedirectToAction("Dashboard", "Admin", new { area = "Admin" });

                if (await _userManager.IsInRoleAsync(user, "Empleado"))
                    return RedirectToAction("Dashboard", "Empleado", new { area = "Empleado" });

                if (await _userManager.IsInRoleAsync(user, "Profesor"))
                    return RedirectToAction("Dashboard", "Profesor", new { area = "Profesor" });

                if (await _userManager.IsInRoleAsync(user, "Estudiante"))
                    return RedirectToAction("Dashboard", "Estudiante", new { area = "Estudiante" });

                return Redirect("/Home/Index");
            }

            ModelState.AddModelError(string.Empty, "Correo o contrasena incorrectos.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpGet]
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroEstudianteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verificar si el email ya existe
            var existingUser = await _userManager.FindByEmailAsync(model.Correo);
            if (existingUser != null)
            {
                ModelState.AddModelError("Correo", "Este correo electrónico ya está registrado.");
                return View(model);
            }

            // Crear usuario de Identity
            var user = new ApplicationUser
            {
                UserName = model.Correo,
                Email = model.Correo,
                NombreCompleto = $"{model.Nombre} {model.Apellido}",
                EmailConfirmed = true,
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Asignar rol de estudiante
                if (!await _roleManager.RoleExistsAsync("Estudiante"))
                    await _roleManager.CreateAsync(new ApplicationRole("Estudiante") { Descripcion = "Rol de Estudiante" });

                await _userManager.AddToRoleAsync(user, "Estudiante");

                // Crear registro de estudiante - USANDO EL NOMBRE COMPLETO DE LA CLASE
                var estudiante = new Models.Entities.Estudiante
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    CI = model.CI,
                    Telefono = model.Telefono,
                    Correo = model.Correo,
                    FechaInscripcion = DateTime.UtcNow,
                    Activo = true,
                    UserId = user.Id
                };

                // Aquí necesitarías inyectar IUnitOfWork para guardar el estudiante
                TempData["SuccessMessage"] = "Registro exitoso. Ahora puede iniciar sesión.";
                _logger.LogInformation($"Nuevo estudiante registrado: {user.Email}");

                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["InfoMessage"] = "Si el correo existe, recibirá instrucciones para restablecer su contraseña.";
                return RedirectToAction("Login");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { token, email = user.Email }, protocol: HttpContext.Request.Scheme);

            _logger.LogInformation($"Token de recuperación para {user.Email}: {token}");

            TempData["InfoMessage"] = "Se han enviado instrucciones a su correo electrónico.";
            return RedirectToAction("Login");
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return RedirectToAction("Login");

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Error al restablecer la contraseña.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Contraseña restablecida exitosamente.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Contraseña cambiada exitosamente.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Método auxiliar
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}