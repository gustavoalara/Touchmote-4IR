using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WiiCPP;
using WiiTUIO.DeviceUtils;
using WiiTUIO.Output;
using WiiTUIO.Properties;
using System.ComponentModel; // Added for INotifyPropertyChanged
using System.Windows.Threading; // Added for Dispatcher

using static WiiTUIO.Resources.Resources; // Importa la clase Resources para acceso directo a las cadenas

namespace WiiTUIO.Provider
{
    /// <summary>
    /// Interaction logic for WiiPointerProviderSettings.xaml
    /// </summary>
    public partial class WiiPointerProviderSettings : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        bool initializing = true;

        private string _newProfileName;
        public string NewProfileName
        {
            get { return _newProfileName; }
            set
            {
                if (_newProfileName != value)
                {
                    _newProfileName = value;
                    OnPropertyChanged("NewProfileName");
                }
            }
        }

        public WiiPointerProviderSettings()
        {
            InitializeComponent();

            this.DataContext = this;

            CalibrationSettings.StaticPropertyChanged += CalibrationSettings_StaticPropertyChanged;

            if (Settings.Default.pointer_4IRMode == "none")
            {
                switch (Settings.Default.pointer_sensorBarPos)
                {
                    case "top":
                        this.cbiTop.IsSelected = true;
                        break;
                    case "bottom":
                        this.cbiBottom.IsSelected = true;
                        break;
                    default:
                        this.cbiCenter.IsSelected = true;
                        break;
                }
            }
            else
            {
                switch (Settings.Default.pointer_4IRMode)
                {
                    case "square":
                        this.cbiSquare.IsSelected = true;
                        break;
                    case "diamond":
                        this.cbiDiamond.IsSelected = true;
                        break;
                    default:
                        this.cbiCenter.IsSelected = true;
                        break;
                }
            }

            this.initializing = false;
        }

        // Manejador para el evento estático de CalibrationSettings.
        private void CalibrationSettings_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Asegurarse de que la actualización de la UI se realice en el hilo de UI.
            Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == "AvailableProfiles")
                {
                    // Forzar la actualización del ItemsSource del ComboBox de perfiles
                    // reasignando directamente la propiedad.
                    ProfileComboBox.ItemsSource = CalibrationSettings.AvailableProfiles;
                }
                else if (e.PropertyName == "ActiveProfileName")
                {
                    // Forzar la actualización del SelectedItem del ComboBox de perfiles
                    // reasignando directamente la propiedad.
                    ProfileComboBox.SelectedItem = CalibrationSettings.ActiveProfileName;
                }
            });
        }


        /// <summary>
        /// Maneja el evento de cambio de selección en el ComboBox de la posición de la barra sensora.
        /// </summary>
        private void SBPositionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.initializing)
            {
                if (this.cbiTop.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "none";
                    Settings.Default.pointer_sensorBarPos = "top";
                }
                else if (this.cbiBottom.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "none";
                    Settings.Default.pointer_sensorBarPos = "bottom";
                }
                else if (this.cbiCenter.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "none";
                    Settings.Default.pointer_sensorBarPos = "center";
                }
                else if (this.cbiSquare.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "square";
                }
                else if (this.cbiDiamond.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "diamond";
                }
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Maneja el evento de cambio de selección en el ComboBox de perfiles de calibración.
        /// </summary>
        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.initializing && ProfileComboBox.SelectedItem != null)
            {
                CalibrationSettings.ActiveProfileName = ProfileComboBox.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Maneja el evento Click del botón "Crear Perfil".
        /// </summary>
        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewProfileName))
            {
                MessageBox.Show(Profile_EnterName, Error_Title_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                CalibrationSettings.CreateNewProfile(NewProfileName);
                MessageBox.Show(string.Format(Profile_CreatedAndActivated, NewProfileName), Success_Title, MessageBoxButton.OK, MessageBoxImage.Information);
                NewProfileName = string.Empty;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Profile_CreateError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Profile_CreateError_Message, ex.Message), Error_Title_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Maneja el evento Click del botón "Eliminar Perfil".
        /// </summary>
        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem == null)
            {
                MessageBox.Show(Profile_SelectToDelete, Error_Title_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string profileToDelete = ProfileComboBox.SelectedItem.ToString();

            if (profileToDelete == "Default Profile")
            {
                MessageBox.Show(Profile_CannotDeleteDefault, Error_Title_Generic, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(string.Format(Profile_ConfirmDeletion_Message, profileToDelete), Profile_ConfirmDeletion_Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    CalibrationSettings.DeleteProfile(profileToDelete);
                    MessageBox.Show(string.Format(Profile_Deleted, profileToDelete), Success_Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, Profile_DeleteError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Profile_DeleteError_Message, ex.Message), Error_Title_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Dispara el evento PropertyChanged para notificar a la UI sobre un cambio en una propiedad de instancia.
        /// </summary>
        /// <param name="name">El nombre de la propiedad que ha cambiado.</param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
