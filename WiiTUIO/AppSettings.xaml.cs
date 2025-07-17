using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
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
using WiiCPP;
using WiiTUIO.DeviceUtils;
using WiiTUIO.Output;
using WiiTUIO.Properties;
using WiiTUIO.Provider;

namespace WiiTUIO
{
    /// <summary>
    /// Interaction logic for AppSettings.xaml
    /// </summary>
    public partial class AppSettingsUC : UserControl, SubPanel
    {

        public event Action OnClose;

        public AppSettingsUC()
        {
            InitializeComponent();
            this.Initialize();
        }

        public async void Initialize()
        {
            Settings.Default.PropertyChanged += Settings_PropertyChanged;

            this.reloadState();

        }

        private async void reloadState()
        {
            this.cbMinimizeOnStart.IsChecked = Settings.Default.minimizeOnStart;
            this.cbMinimizeToTray.IsChecked = Settings.Default.minimizeToTray;
            this.cbPairOnStart.IsChecked = Settings.Default.pairOnStart;
            this.providerSettingsContent.Children.Clear();
            this.providerSettingsContent.Children.Add(MultiWiiPointerProvider.getSettingsControl());

            this.cbWindowsStart.IsChecked = Autostart.IsAutostart();
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                this.reloadState();
            }), null);
        }

        private async void cbWindowsStart_Checked(object sender, RoutedEventArgs e)
        {
            this.cbWindowsStart.IsChecked = Autostart.SetAutostart();
        }

        private async void cbWindowsStart_Unchecked(object sender, RoutedEventArgs e)
        {
            this.cbWindowsStart.IsChecked = !(Autostart.UnsetAutostart());
        }

        private void btnAppSettingsBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnClose != null)
            {
                this.OnClose();
            }
        }

        private void cbPairOnStart_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.pairOnStart = true;
        }

        private void cbPairOnStart_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.pairOnStart = false;
        }

        private void cbDisconnectOnExit_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.completelyDisconnect = true;
        }

        private void cbDisconnectOnExit_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.completelyDisconnect = false;
        }

        private void cbMinimizeToTray_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.minimizeToTray = true;
        }

        private void cbMinimizeToTray_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.minimizeToTray = false;
        }

        private void cbMinimizeOnStart_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.minimizeOnStart = true;
        }

        private void cbMinimizeOnStart_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.minimizeOnStart = false;
        }

        private void btnEditKeymaps_Click(object sender, RoutedEventArgs e)
        {
            KeymapConfigWindow.Instance.Show();
        }

        private void btnAdvConfig_Click(object sender, RoutedEventArgs e)
        {
            NewAppSettingsUC.Instance.Show(); // Llama a la instancia única de la ventana de configuración avanzada
        }

    }
}
