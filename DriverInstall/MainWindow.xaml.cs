using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
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

namespace DriverInstall
{
    public partial class MainWindow : Window
    {
        private bool shutdown = false;

        public MainWindow()
        {
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

        private void consoleLine(string text) {
            this.console.Text += "\n";
            this.console.Text += text;
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
            this.uninstallVmultiDriver();
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
            consoleLine("Adding Touchmote Test Certificate to trusted root.");
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
            consoleLine("Removing Touchmote Test Certificate.");
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
            this.uninstallVmultiDriver();
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
                    Console.WriteLine(result);
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    Console.WriteLine(result);
                    proc.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            this.installVmultiDriverComplete();
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            this.uninstallVmultiDriverComplete();
        }

    }
}
