using System;
using System.Reflection;
using System.Resources;
using System.Windows.Markup; // Necesario para MarkupExtension
using System.Globalization; // Necesario para CultureInfo

namespace DriverInstall // ¡IMPORTANTE! Asegúrate de que este sea el Namespace de tu proyecto
{
    public class LocalizationExtension : MarkupExtension
    {
        public string Name { get; set; }

        public LocalizationExtension(string name)
        {
            Name = name;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Ajusta el Namespace.NombreDelArchivoDeRecursos según dónde estén tus archivos .resx
            // Si tus archivos .resx están en la carpeta 'Properties', usa "DriverInstall.Properties.Resources"
            // Si están directamente en la raíz del proyecto, usa "DriverInstall.Resources"
            ResourceManager resourceManager = new ResourceManager("DriverInstall.Resources.Resources", Assembly.GetExecutingAssembly());

            // Obtiene la cadena del recurso para la cultura actual de la UI
            string localizedString = resourceManager.GetString(Name, CultureInfo.CurrentUICulture);

            // Si no se encuentra la cadena, devuelve la clave para facilitar la depuración
            if (localizedString == null)
            {
                return $"!{Name}!";
            }

            return localizedString;
        }
    }
}