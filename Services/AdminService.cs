using Microsoft.AspNetCore.Identity;
using SistemaGestionAcademica.Models.Entities;
using SistemaGestionAcademica.Repositories.Interfaces;
using SistemaGestionAcademica.Services.Interfaces;

namespace SistemaGestionAcademica.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExcelExportService _excelService;

        public AdminService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IExcelExportService excelService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _excelService = excelService;
        }

        // ============ PROFESORES ============

        public async Task<Profesor> CrearProfesorAsync(Profesor profesor, string email, string password)
        {
            // Verificar CI único
            if (await _unitOfWork.Profesores.GetByCIAsync(profesor.CI) != null)
                throw new InvalidOperationException("Ya existe un profesor con esta cédula");

            // Crear usuario
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NombreCompleto = $"{profesor.Nombre} {profesor.Apellido}",
                EmailConfirmed = true,
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Profesor");

            profesor.UserId = user.Id;
            profesor.FechaContratacion = DateTime.UtcNow;
            profesor.Activo = true;

            await _unitOfWork.Profesores.AddAsync(profesor);
            await _unitOfWork.CompleteAsync();

            return profesor;
        }

        public async Task<Profesor> ActualizarProfesorAsync(Profesor profesor)
        {
            var existing = await _unitOfWork.Profesores.GetByIdAsync(profesor.Id);
            if (existing == null)
                throw new InvalidOperationException("Profesor no encontrado");

            existing.Nombre = profesor.Nombre;
            existing.Apellido = profesor.Apellido;
            existing.Telefono = profesor.Telefono;
            existing.Especialidad = profesor.Especialidad;
            existing.Correo = profesor.Correo;

            await _unitOfWork.Profesores.UpdateAsync(existing);
            await _unitOfWork.CompleteAsync();

            return existing;
        }

        public async Task<bool> DarBajaProfesorAsync(int id)
        {
            var profesor = await _unitOfWork.Profesores.GetByIdAsync(id);
            if (profesor == null) return false;

            profesor.Activo = false;
            profesor.FechaBaja = DateTime.UtcNow;
            await _unitOfWork.Profesores.UpdateAsync(profesor);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ContratarProfesorAsync(int id)
        {
            var profesor = await _unitOfWork.Profesores.GetByIdAsync(id);
            if (profesor == null) return false;

            profesor.Activo = true;
            profesor.FechaBaja = null;
            profesor.FechaContratacion = DateTime.UtcNow;
            await _unitOfWork.Profesores.UpdateAsync(profesor);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        // ============ EMPLEADOS ============

        public async Task<Empleado> CrearEmpleadoAsync(Empleado empleado, string email, string password)
        {
            if (await _unitOfWork.Empleados.GetByCIAsync(empleado.CI) != null)
                throw new InvalidOperationException("Ya existe un empleado con esta cédula");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NombreCompleto = $"{empleado.Nombre} {empleado.Apellido}",
                EmailConfirmed = true,
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Empleado");

            empleado.UserId = user.Id;
            empleado.FechaContratacion = DateTime.UtcNow;
            empleado.Activo = true;

            await _unitOfWork.Empleados.AddAsync(empleado);
            await _unitOfWork.CompleteAsync();

            return empleado;
        }

        public async Task<Empleado> ActualizarEmpleadoAsync(Empleado empleado)
        {
            var existing = await _unitOfWork.Empleados.GetByIdAsync(empleado.Id);
            if (existing == null)
                throw new InvalidOperationException("Empleado no encontrado");

            existing.Nombre = empleado.Nombre;
            existing.Apellido = empleado.Apellido;
            existing.Telefono = empleado.Telefono;
            existing.Cargo = empleado.Cargo;
            existing.Correo = empleado.Correo;

            await _unitOfWork.Empleados.UpdateAsync(existing);
            await _unitOfWork.CompleteAsync();

            return existing;
        }

        public async Task<bool> DarBajaEmpleadoAsync(int id)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(id);
            if (empleado == null) return false;

            empleado.Activo = false;
            empleado.FechaBaja = DateTime.UtcNow;
            await _unitOfWork.Empleados.UpdateAsync(empleado);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        // ============ MATERIAS ============

        public async Task<Materia> CrearMateriaAsync(Materia materia)
        {
            materia.Activo = true;
            await _unitOfWork.Materias.AddAsync(materia);
            await _unitOfWork.CompleteAsync();
            return materia;
        }

        public async Task<Materia> ActualizarMateriaAsync(Materia materia)
        {
            await _unitOfWork.Materias.UpdateAsync(materia);
            await _unitOfWork.CompleteAsync();
            return materia;
        }

        // ============ AULAS ============

        public async Task<Aula> CrearAulaAsync(Aula aula)
        {
            aula.Activo = true;
            await _unitOfWork.Aulas.AddAsync(aula);
            await _unitOfWork.CompleteAsync();
            return aula;
        }

        public async Task<Aula> ActualizarAulaAsync(Aula aula)
        {
            await _unitOfWork.Aulas.UpdateAsync(aula);
            await _unitOfWork.CompleteAsync();
            return aula;
        }

        // ============ CONFIGURACIÓN ============

        public async Task<ConfiguracionInstitucional> GetConfiguracionAsync()
        {
            return await _unitOfWork.Configuraciones.GetConfiguracionActualAsync()
                ?? throw new InvalidOperationException("No hay configuración activa");
        }

        public async Task ActualizarConfiguracionAsync(ConfiguracionInstitucional config, string usuarioId)
        {
            await _unitOfWork.Configuraciones.ActualizarConfiguracionAsync(config, usuarioId);
        }

        // ============ REPORTES ============

        public Task<byte[]> ExportarListadoEstudiantesAsync() => _excelService.ExportarListadoEstudiantesAsync();
        public Task<byte[]> ExportarListadoProfesoresAsync() => _excelService.ExportarListadoProfesoresAsync();
        public Task<byte[]> ExportarReportePagosAsync(DateTime? inicio, DateTime? fin) => _excelService.ExportarReportePagosAsync(inicio, fin);
        public Task<byte[]> ExportarEstudiantesConDeudaAsync() => _excelService.ExportarEstudiantesConDeudaAsync();
    }
}