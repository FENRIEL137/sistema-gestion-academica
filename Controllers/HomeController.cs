using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionAcademica.Models.ViewModels;
using SistemaGestionAcademica.Models.Entities;
using System.Diagnostics;

namespace SistemaGestionAcademica.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                // Redirigir al dashboard según el rol
                if (User.IsInRole("Administrador"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                else if (User.IsInRole("Empleado"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Empleado" });
                else if (User.IsInRole("Profesor"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Profesor" });
                else if (User.IsInRole("Estudiante"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Estudiante" });
            }

            return View();
        }

        public IActionResult Dashboard()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador"))
                    return RedirectToAction("Dashboard", "Admin", new { area = "Admin" });

                if (User.IsInRole("Profesor"))
                    return RedirectToAction("Dashboard", "Profesor", new { area = "Profesor" });

                if (User.IsInRole("Estudiante"))
                    return RedirectToAction("Dashboard", "Estudiante", new { area = "Estudiante" });

                if (User.IsInRole("Empleado"))
                    return RedirectToAction("Dashboard", "Empleado", new { area = "Empleado" });
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}