namespace SistemaGestionAcademica.Models.ViewModels
{
    /// <summary>
    /// Modelo para items de breadcrumb (migas de pan)
    /// </summary>
    public class BreadcrumbItem
    {
        /// <summary>
        /// Texto a mostrar en el breadcrumb
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// URL del enlace (opcional, si es null se muestra como texto plano)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Indica si es el item activo (último en la cadena)
        /// </summary>
        public bool Active { get; set; } = false;

        /// <summary>
        /// Icono de Bootstrap (opcional)
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Constructor vacío
        /// </summary>
        public BreadcrumbItem()
        {
        }

        /// <summary>
        /// Constructor con parámetros básicos
        /// </summary>
        public BreadcrumbItem(string text, string? url = null, bool active = false)
        {
            Text = text;
            Url = url;
            Active = active;
        }
    }
}