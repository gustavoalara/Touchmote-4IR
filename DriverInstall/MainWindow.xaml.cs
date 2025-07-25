using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading; // Added for Thread.CurrentThread
using System.Globalization; // Added for CultureInfo
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using static DriverInstall.Resources.Resources;

namespace DriverInstall
{
    public partial class MainWindow : Window
    {
        private bool shutdown = false;

        public MainWindow()
        {
            // --- INICIO DE LOS CAMBIOS PARA LOCALIZACIÓN ---

            // Obtiene la cultura actual de la UI del sistema operativo.
            CultureInfo currentSystemUICulture = CultureInfo.CurrentUICulture;

            // Lista de culturas soportadas por tu aplicación (ej: "es-ES", "en-US", "fr-FR").
            // Asegúrate de que los nombres coincidan con tus archivos .resx (ej: Resources.es-ES.resx, Resources.fr-FR.resx).
            // Puedes ser más específico (ej: "fr-FR") o más general (ej: "fr").
            // Si usas "fr", ResourceManager buscará Resources.fr.resx y, si no lo encuentra, buscará Resources.fr-FR.resx
            // si existe, y así sucesivamente, antes de recurrir al idioma por defecto.
            string[] supportedCultures = { "es", "en", "fr" }; // Puedes añadir más según necesites

            // Busca si el idioma del sistema está entre las culturas soportadas.
            // currentSystemUICulture.Name es el nombre completo (ej: "es-ES", "fr-FR").
            // currentSystemUICulture.TwoLetterISOLanguageName es solo el código de dos letras (ej: "es", "fr").
            // Preferimos usar el nombre de dos letras para una coincidencia más flexible primero.
            string cultureToUse = "en-US"; // Establece "en-US" como cultura por defecto si ninguna coincide

            // Verifica si la cultura del sistema es directamente soportada o si su idioma base lo es.
            bool cultureFound = false;
            foreach (string supportedCulture in supportedCultures)
            {
                if (currentSystemUICulture.Name.StartsWith(supportedCulture, StringComparison.OrdinalIgnoreCase) ||
                    currentSystemUICulture.TwoLetterISOLanguageName.Equals(supportedCulture, StringComparison.OrdinalIgnoreCase))
                {
                    cultureToUse = currentSystemUICulture.Name;
                    cultureFound = true;
                    break; // Se encontró una coincidencia, no necesitamos seguir buscando
                }
            }

            if (!cultureFound)
            {
                // Si el idioma del sistema no es soportado directamente,
                // intentamos ver si el idioma genérico (ej: "fr" para "fr-CA") está soportado.
                // Si no, se mantendrá "en-US" como valor por defecto.
                bool baseCultureFound = false;
                foreach (string supportedCulture in supportedCultures)
                {
                    if (currentSystemUICulture.TwoLetterISOLanguageName.Equals(supportedCulture, StringComparison.OrdinalIgnoreCase))
                    {
                        cultureToUse = currentSystemUICulture.TwoLetterISOLanguageName; // Usar el idioma base (ej: "fr")
                        baseCultureFound = true;
                        break;
                    }
                }

                if (!baseCultureFound)
                {
                    // Si ni la cultura específica ni la base son soportadas,
                    // verificamos si la cultura por defecto (ej: "en") es una de las soportadas
                    // y si el sistema está en esa cultura. Si no, simplemente se usa "en-US".
                    if (currentSystemUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
                    {
                        cultureToUse = "en-US";
                    }
                    // Si el idioma del sistema no es inglés y no está en supportedCultures,
                    // 'cultureToUse' seguirá siendo "en-US", que es el comportamiento deseado para el fallback.
                }
            }

            // Establece la cultura de la interfaz de usuario (UI Culture) para la aplicación.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureToUse);

            // También establece la cultura general del hilo (afecta formatos de fecha, números, etc.)
            // para que coincida con la cultura de la UI.
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;

            // --- FIN DE LOS CAMBIOS PARA LOCALIZACIÓN ---

            InitializeComponent();

            if (Environment.GetCommandLineArgs().Contains("-silent"))
            {
                this.Visibility = Visibility.Hidden;
                this.shutdown = true;
            }

            if (Environment.GetCommandLineArgs().Contains("-install"))
            {
                if (Environment.GetCommandLineArgs().Contains("-vmulti"))
                {
                    this.installVmultiDriverComplete();
                }

                if (Environment.GetCommandLineArgs().Contains("-certificate"))
                {
                    this.installCert();
                }
            }
            else if (Environment.GetCommandLineArgs().Contains("-uninstall"))
            {

                if (Environment.GetCommandLineArgs().Contains("-vmulti"))
                {
                    this.uninstallVmultiDriverComplete();
                }

                if (Environment.GetCommandLineArgs().Contains("-certificate"))
                {
                    this.uninstallCert();
                }
            }

            if (shutdown)
            {
                Application.Current.Shutdown(1);
            }
        }

        private void consoleLine(string text)
        {
            this.console.Text += "\n";
            this.console.Text += text;
            // Desplazar el ScrollViewer al final para ver los mensajes más recientes
            this.console.ScrollToEnd();
        }

        private void installAll()
        {
            this.installVmultiDriverComplete();
            this.uninstallCert();
            this.installCert();
        }

        private void uninstallAll()
        {
            this.uninstallVmultiDriverComplete();
            this.uninstallCert();
        }

        private void installVmultiDriverComplete()
        {
            this.uninstallVmultiDriver();
            this.uninstallVmultiDriver(); // Llamada duplicada, ¿intencional?
            this.installVmultiDrivers();

            this.removeAllButMKB();
        }

        private void installCert()
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            StorePermission sp = new StorePermission(PermissionState.Unrestricted);
            sp.Flags = StorePermissionFlags.OpenStore;
            sp.Assert();
            store.Open(OpenFlags.ReadWrite);
            X509Certificate2Collection collection = new X509Certificate2Collection();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "CodeSign.cer";
            X509Certificate2 cert = new X509Certificate2(path);
            byte[] encodedCert = cert.GetRawCertData();
            // --- LOCALIZACIÓN PENDIENTE: Este string debería venir de los recursos ---
            consoleLine(AddingCertMsg);
            store.Add(cert);
            store.Close();
        }

        private void uninstallCert()
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            StorePermission sp = new StorePermission(PermissionState.Unrestricted);
            sp.Flags = StorePermissionFlags.OpenStore;
            sp.Assert();
            store.Open(OpenFlags.ReadWrite);
            // --- LOCALIZACIÓN PENDIENTE: Este string debería venir de los recursos ---
            consoleLine(RemovingCertMsg);
            foreach (X509Certificate2 c in store.Certificates)
            {
                if (c.IssuerName.Name.Contains("Touchmote"))
                {
                    store.Remove(c);
                }
            }
            store.Close();
        }

        private void uninstallVmultiDriverComplete()
        {
            this.uninstallVmultiDriver();
            this.uninstallVmultiDriver(); // Llamada duplicada, ¿intencional?
        }

        private void installVmultiDrivers()
        {
            try
            {
                string[] drivers = { "vmultia", "vmultib", "vmultic", "vmultid" };

                foreach (string driver in drivers)
                {
                    System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "Driver\\",
                        FileName = System.AppDomain.CurrentDomain.BaseDirectory + "Driver\\devcon",
                        Arguments = $"install {driver}.inf ecologylab\\{driver}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    System.Diagnostics.Process proc = new System.Diagnostics.Process
                    {
                        StartInfo = procStartInfo
                    };

                    proc.Start();
                    string result = proc.StandardOutput.ReadToEnd();
                    Console.WriteLine(result); // Considerar enviar esto también a consoleLine
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Considerar enviar esto también a consoleLine
                // --- LOCALIZACIÓN PENDIENTE: El mensaje de error también podría ser localizado o formateado ---
                consoleLine(string.Format(ErrorVmultiInstall, ex.Message));
            }
        }

        private void uninstallVmultiDriver()
        {
            try
            {
                //Devcon remove *multi*
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo();

                procStartInfo.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "Driver\\";

                procStartInfo.FileName = procStartInfo.WorkingDirectory + "devcon";
                procStartInfo.Arguments = "remove *vmulti*";

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                string result = proc.StandardOutput.ReadToEnd();
                consoleLine(result);
                proc.WaitForExit();
            }
            catch (Exception objException)
            {
                consoleLine(objException.Message);
                // --- LOCALIZACIÓN PENDIENTE: El mensaje de error también podría ser localizado o formateado ---
                consoleLine(string.Format(ErrorVmultiUninstall, objException.Message ));
            }
        }

        private void removeAllButMKB()
        {
            int[] drivers = { 1, 2, 4, 5, 6 };

            foreach (int i in drivers)
            {
                try
                {
                    // Setup the process start info
                    System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "Driver\\",
                        FileName = System.AppDomain.CurrentDomain.BaseDirectory + "Driver\\devcon",
                        Arguments = "disable *vmulti*COL0" + i + "*",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    // Create and start the process
                    System.Diagnostics.Process proc = new System.Diagnostics.Process
                    {
                        StartInfo = procStartInfo
                    };

                    proc.Start();
                    string result = proc.StandardOutput.ReadToEnd();
                    Console.WriteLine(result); // Considerar enviar esto también a consoleLine
                    proc.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message); // Considerar enviar esto también a consoleLine
                    // --- LOCALIZACIÓN PENDIENTE: El mensaje de error también podría ser localizado o formateado ---
                    consoleLine(string.Format(ErrorDisablingDriver,  ex.Message));
                }
            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            this.installVmultiDriverComplete();
            // --- LOCALIZACIÓN PENDIENTE: Este string debería venir de los recursos ---
            consoleLine(DriverInstallSuccesfull);
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            this.uninstallVmultiDriverComplete();
            // --- LOCALIZACIÓN PENDIENTE: Este string debería venir de los recursos ---
            consoleLine(DriverUninstallSuccessfull);
        }
    }
}