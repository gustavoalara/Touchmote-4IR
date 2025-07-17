using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel; 
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using WiiTUIO.DeviceUtils; 

namespace WiiTUIO
{
    /// <summary>
    /// Interaction logic for NewAppSettingsUC.xaml
    /// This MetroWindow provides a UI for editing application settings,
    /// loading from and saving to settings.json via SettingsManager.
    /// </summary>
    public partial class NewAppSettingsUC : MetroWindow
    {
        // Implementación del patrón Singleton para NewAppSettingsUC
        private static NewAppSettingsUC defaultInstance;
        public static NewAppSettingsUC Instance
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new NewAppSettingsUC();
                }
                return defaultInstance;
            }
        }
        /// <summary>
        /// Constructor privado para asegurar que solo una instancia de la ventana pueda ser creada.
        /// </summary>
        private NewAppSettingsUC() 
        {
            InitializeComponent();
            // Set the DataContext to the current settings loaded by the SettingsManager.
            // This enables two-way data binding between UI controls and the SettingsData properties.
            this.DataContext = SettingsManager.CurrentSettings;
            
        }
        

        /// <summary>
        /// Maneja el evento Click para el botón "Volver".
        /// Guarda la configuración y oculta la MetroWindow en lugar de cerrarla.
        /// </summary>
        private void BtnAppSettingsBack_Click(object sender, RoutedEventArgs e)
        {
            // Guarda la configuración antes de ocultar la ventana
            SettingsManager.SaveSettings();
            this.Hide(); // Oculta la MetroWindow en lugar de cerrarla para reutilizar la instancia
        }
        /// <summary>
        /// Maneja el evento Click para el botón de búsqueda de la ruta de Dolphin.
        /// </summary>
        private void BtnBrowseDolphinPath_Click(object sender, RoutedEventArgs e)
        {
            // Se utiliza FolderBrowserDialog para seleccionar una carpeta en lugar de un archivo.
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Seleccionar Carpeta de Dolphin"; // Título del diálogo


            // Establece el directorio inicial si ya hay una ruta guardada
            if (!string.IsNullOrEmpty(SettingsManager.CurrentSettings.DolphinPath))
            {
                try
                {
                    // Verifica si la ruta guardada es un directorio válido
                    if (System.IO.Directory.Exists(SettingsManager.CurrentSettings.DolphinPath))
                    {
                        folderBrowserDialog.SelectedPath = SettingsManager.CurrentSettings.DolphinPath;
                    }
                    else // Si la ruta guardada es un archivo, intenta obtener el directorio padre
                    {
                        string initialDirectory = System.IO.Path.GetDirectoryName(SettingsManager.CurrentSettings.DolphinPath);
                        if (System.IO.Directory.Exists(initialDirectory))
                        {
                            folderBrowserDialog.SelectedPath = initialDirectory;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al establecer el directorio inicial para DolphinPath: {ex.Message}");
                }
            }

            // Muestra el diálogo y comprueba si el usuario seleccionó una carpeta
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                // Asegura que la ruta termine con el separador de directorio
                if (!selectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) && !selectedPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                {
                    selectedPath += System.IO.Path.DirectorySeparatorChar;
                }
                SettingsManager.CurrentSettings.DolphinPath = selectedPath;
                SettingsManager.SaveSettings();
            }
        }
        /// <summary>
        /// Maneja el evento Click para el botón de búsqueda de la ruta de Keymaps.
        /// </summary>
        private void BtnBrowseKeyMapsPath_Click(object sender, RoutedEventArgs e)
        {
            // Se utiliza FolderBrowserDialog para seleccionar una carpeta en lugar de un archivo.
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Seleccionar Carpeta de Keymaps"; // Título del diálogo


            // Establece el directorio inicial si ya hay una ruta guardada
            if (!string.IsNullOrEmpty(SettingsManager.CurrentSettings.KeymapsPath))
            {
                try
                {
                    // Verifica si la ruta guardada es un directorio válido
                    if (System.IO.Directory.Exists(SettingsManager.CurrentSettings.KeymapsPath))
                    {
                        folderBrowserDialog.SelectedPath = SettingsManager.CurrentSettings.KeymapsPath;
                    }
                    else // Si la ruta guardada es un archivo, intenta obtener el directorio padre
                    {
                        string initialDirectory = System.IO.Path.GetDirectoryName(SettingsManager.CurrentSettings.KeymapsPath);
                        if (System.IO.Directory.Exists(initialDirectory))
                        {
                            folderBrowserDialog.SelectedPath = initialDirectory;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al establecer el directorio inicial para KeymapsPath: {ex.Message}");
                }
            }

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                // Asegura que la ruta termine con el separador de directorio
                if (!selectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) && !selectedPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                {
                    selectedPath += System.IO.Path.DirectorySeparatorChar;
                }
                SettingsManager.CurrentSettings.KeymapsPath = selectedPath;
                SettingsManager.SaveSettings();
            }
        }
        /// <summary>
        /// Maneja el evento Click para el botón de búsqueda del archivo de configuración de Keymaps.
        /// Abre un OpenFileDialog para que el usuario seleccione el archivo Keymaps.json.
        /// </summary>
        private void BtnBrowseKeymapsConfig_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos JSON (*.json)|*.json|Todos los Archivos (*.*)|*.*",
                Title = "Seleccionar Archivo de Configuración de Keymaps"
            };

            // Intenta establecer el directorio inicial basándose en la ruta de Keymaps o la ruta actual del archivo.
            string initialDirectory = SettingsManager.CurrentSettings.KeymapsPath;
            if (!string.IsNullOrEmpty(SettingsManager.CurrentSettings.KeymapsConfig) && System.IO.File.Exists(System.IO.Path.Combine(SettingsManager.CurrentSettings.KeymapsPath, SettingsManager.CurrentSettings.KeymapsConfig)))
            {
                openFileDialog.FileName = SettingsManager.CurrentSettings.KeymapsConfig; // Establece el nombre de archivo predeterminado
            }

            try
            {
                if (System.IO.Directory.Exists(initialDirectory))
                {
                    openFileDialog.InitialDirectory = initialDirectory;
                }
                else if (!string.IsNullOrEmpty(SettingsManager.CurrentSettings.KeymapsConfig) && System.IO.File.Exists(SettingsManager.CurrentSettings.KeymapsConfig))
                {
                    // Si KeymapsConfig es una ruta absoluta válida, usa su directorio
                    openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(SettingsManager.CurrentSettings.KeymapsConfig);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al establecer el directorio inicial para KeymapsConfig: {ex.Message}");
            }

            if (openFileDialog.ShowDialog() == true)
            {
                // Almacena solo el nombre del archivo, no la ruta completa
                SettingsManager.CurrentSettings.KeymapsConfig = System.IO.Path.GetFileName(openFileDialog.FileName);
                SettingsManager.SaveSettings();
            }
        }
        /// <summary>
        /// Maneja los eventos Checked/Unchecked para los controles CheckBox.
        /// Guarda la configuración cada vez que cambia el estado de un checkbox.
        /// </summary>
        private void Setting_Changed(object sender, RoutedEventArgs e)
        {
            SettingsManager.SaveSettings();
        }

        /// <summary>
        /// Maneja el evento LostFocus para los controles TextBox.
        /// Se utiliza para los cuadros de texto de cadena y numéricos para guardar los cambios
        /// cuando el usuario mueve el foco fuera del cuadro de texto.
        /// </summary>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // El binding actualiza automáticamente la propiedad de origen, así que solo necesitamos guardar.
            SettingsManager.SaveSettings();
        }

        /// <summary>
        /// Maneja el evento ValueChanged para los controles Slider.
        /// Guarda la configuración cada vez que cambia el valor de un slider.
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // El binding actualiza automáticamente la propiedad de origen, así que solo necesitamos guardar.
            SettingsManager.SaveSettings();
        }

        /// <summary>
        /// Maneja el evento LostFocus para los cuadros de texto de componentes de color (R, G, B).
        /// Analiza el texto y actualiza el array de color correspondiente en SettingsData.
        /// </summary>
        private void ColorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            // La propiedad Tag se utiliza para identificar qué ID de color y componente (R,G,B) representa este cuadro de texto.
            // Formato: "ColorID[N]_[ComponentIndex]" ej., "ColorID1_0" para el componente Rojo de ColorID1.
            string tag = textBox.Tag?.ToString();
            if (string.IsNullOrEmpty(tag)) return;

            // Analiza el nuevo valor del cuadro de texto
            if (int.TryParse(textBox.Text, out int value))
            {
                // Asegura que el valor esté dentro del rango RGB válido [0, 255]
                value = Math.Max(0, Math.Min(255, value));
                textBox.Text = value.ToString(); // Actualiza el cuadro de texto con el valor ajustado

                SettingsData currentSettings = SettingsManager.CurrentSettings;

                // Usa regex para extraer ColorID e índice
                Match match = Regex.Match(tag, @"ColorID(\d)_(\d)");
                if (match.Success)
                {
                    int colorId = int.Parse(match.Groups[1].Value);
                    int componentIndex = int.Parse(match.Groups[2].Value);

                    List<int> targetColorList = null;

                    // Determina qué lista de ColorID actualizar
                    switch (colorId)
                    {
                        case 1: targetColorList = currentSettings.ColorID1; break;
                        case 2: targetColorList = currentSettings.ColorID2; break;
                        case 3: targetColorList = currentSettings.ColorID3; break;
                        case 4: targetColorList = currentSettings.ColorID4; break;
                    }

                    if (targetColorList != null && componentIndex >= 0 && componentIndex < targetColorList.Count)
                    {
                        // Actualiza el componente de color específico
                        targetColorList[componentIndex] = value;
                        // Para List<int>, PropertyChanged no se disparará al cambiar un elemento.
                        // Guardar la configuración aquí asegura la persistencia.
                    }
                }
            }
            else
            {
                Console.WriteLine($"Entrada inválida para el componente de color: {textBox.Text}");
            }
            SettingsManager.SaveSettings(); // Siempre guarda después de intentar una actualización
        }

        /// <summary>
        /// Evita la entrada no numérica en los cuadros de texto destinados a valores enteros.
        /// Permite solo dígitos y caracteres de control (como el retroceso).
        /// </summary>
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        /// <summary>
        /// Evita la entrada no numérica (excepto un solo punto decimal) en los cuadros de texto destinados a valores dobles.
        /// </summary>
        private void DoubleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            // Permite dígitos y un solo separador decimal basado en la cultura actual
            bool isDigit = char.IsDigit(e.Text, 0);
            bool isDecimal = e.Text == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            // Si es un separador decimal, asegura que solo exista uno en el cuadro de texto
            if (isDecimal && textBox.Text.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
            {
                e.Handled = true;
            }
            // Si no es un dígito y no es un separador decimal, lo maneja (evita la entrada)
            else if (!isDigit && !isDecimal)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Sobrescribe el método OnClosing para ocultar la ventana en lugar de cerrarla,
        /// permitiendo que la instancia se reutilice.
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true; // Cancela el evento de cierre
            this.Hide();     // Oculta la ventana
            base.OnClosing(e);
        }
        // Implementación de INotifyPropertyChanged para las propiedades del ViewModel
        public event PropertyChangedEventHandler PropertyChanged;
        // Renombrado de OnPropertyChanged a RaisePropertyChanged para evitar ambigüedad con DependencyObject.OnPropertyChanged
        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
