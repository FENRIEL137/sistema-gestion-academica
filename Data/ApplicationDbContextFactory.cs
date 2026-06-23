using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SistemaGestionAcademica.Data
{
    /// <summary>
    /// Factory para crear el DbContext en tiempo de diseño
    /// Necesario para que EF Core Tools pueda generar migraciones
    /// sin necesidad de ejecutar la aplicación
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Construir la configuración desde appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            // Configurar las opciones del contexto
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Obtener la cadena de conexión
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configurar SQL Server
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("SistemaGestionAcademica");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}