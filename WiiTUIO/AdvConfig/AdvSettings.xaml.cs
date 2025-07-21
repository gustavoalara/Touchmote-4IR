using System;
using System.Collections.Generic;
using System.ComponentModel; // Added for CancelEventArgs
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.Windows.Forms; // For FolderBrowserDialog
using System.IO; // For Path
using System.Diagnostics; // For Process.Start
using System.Runtime.CompilerServices; // For CallerMemberName
using System.Threading; // Added for Thread.Sleep
using WiiTUIO.Properties; // Added to access the original Settings class
using System.Globalization;

using static WiiTUIO.Resources.Resources;

namespace WiiTUIO
{
    /// <summary>
    /// Interaction logic for AdvSettings.xaml
    /// This MetroWindow provides a UI for editing application settings,
    /// loading from and saving to settings.json via the original Settings class.
    /// </summary>
    public partial class AdvSettingsUC : MetroWindow, INotifyPropertyChanged // Renamed class to AdvSettingsUC
    {
        // Implementation of the Singleton pattern for AdvSettingsUC
        private static AdvSettingsUC defaultInstance;
        public static AdvSettingsUC Instance
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new AdvSettingsUC();
                }
                return defaultInstance;
            }
        }

        // Implementation of INotifyPropertyChanged for ViewModel properties
        public event PropertyChangedEventHandler PropertyChanged;

        // Renamed OnPropertyChanged to RaisePropertyChanged to avoid ambiguity with DependencyObject.OnPropertyChanged
        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Private constructor to ensure only one instance of the window can be created.
        /// </summary>
        private AdvSettingsUC()
        {
            InitializeComponent();
            // Sets the DataContext of the window to the Default instance of the original Settings class
            this.DataContext = Settings.Default;
        }

        /// <summary>
        /// Handles the Click event for the "Back" button.
        /// Hides the MetroWindow without saving changes.
        /// </summary>
        private void BtnAppSettingsBack_Click(object sender, RoutedEventArgs e)
        {
            // Changes are not saved. Simply hide the window.
            this.Hide();
        }

        /// <summary>
        /// Handles the Click event for the "Save and Restart" button.
        /// Saves the current configuration and restarts the application.
        /// </summary>
        private void BtnSaveAndRestart_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("BtnSaveAndRestart_Click: Saving configuration...");
            // Calls the Save method of the original Settings class
            Settings.Default.Save();

            Debug.WriteLine("BtnSaveAndRestart_Click: Attempting to restart the application...");
            try
            {
                string executablePath = System.Windows.Application.ResourceAssembly.Location;
                Debug.WriteLine($"BtnSaveAndRestart_Click: Executable path: {executablePath}");

                // Creates a new ProcessStartInfo for the restart
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true, // Important for the operating system to handle the launch
                    WorkingDirectory = Path.GetDirectoryName(executablePath), // Sets the working directory
                    Arguments = "--restarting" // Adds the argument to indicate it's a restart
                };

                Process.Start(startInfo);
                Debug.WriteLine("BtnSaveAndRestart_Click: New process launched with argument --restarting. Closing current application...");

                // A small delay to ensure the new process has time to start
                Thread.Sleep(500);

                System.Windows.Application.Current.Shutdown();
                Debug.WriteLine("BtnSaveAndRestart_Click: Current application closed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BtnSaveAndRestart_Click: Error restarting the application: {ex.Message}. StackTrace: {ex.StackTrace}");
                System.Windows.MessageBox.Show(string.Format(RestartError, ex.Message), "Restart Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event for the Dolphin path browse button.
        /// </summary>
        private void BtnBrowseDolphinPath_Click(object sender, RoutedEventArgs e)
        {
            // FolderBrowserDialog is used to select a folder instead of a file.
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = DolphinFolder; // Dialog title

            // Sets the initial directory if a path is already saved
            if (!string.IsNullOrEmpty(Settings.Default.dolphin_path))
            {
                try
                {
                    // Checks if the saved path is a valid directory
                    if (System.IO.Directory.Exists(Settings.Default.dolphin_path))
                    {
                        folderBrowserDialog.SelectedPath = Settings.Default.dolphin_path;
                    }
                    else // If the saved path is a file, try to get the parent directory
                    {
                        string initialDirectory = System.IO.Path.GetDirectoryName(Settings.Default.dolphin_path);
                        if (System.IO.Directory.Exists(initialDirectory))
                        {
                            folderBrowserDialog.SelectedPath = initialDirectory;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting initial directory for DolphinPath: {ex.Message}");
                }
            }

            // Shows the dialog and checks if the user selected a folder
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                // Ensures the path ends with the directory separator
                if (!selectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) && !selectedPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                {
                    selectedPath += System.IO.Path.DirectorySeparatorChar;
                }
                Settings.Default.dolphin_path = selectedPath;
                // Not saved here, it's saved with the Save & Restart button
            }
        }

        /// <summary>
        /// Handles the Click event for the Keymaps path browse button.
        /// </summary>
        private void BtnBrowseKeyMapsPath_Click(object sender, RoutedEventArgs e)
        {
            // FolderBrowserDialog is used to select a folder instead of a file.
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = KeymapsFolder; // Dialog title

            // Sets the initial directory if a path is already saved
            if (!string.IsNullOrEmpty(Settings.Default.keymaps_path))
            {
                try
                {
                    // Checks if the saved path is a valid directory
                    if (System.IO.Directory.Exists(Settings.Default.keymaps_path))
                    {
                        folderBrowserDialog.SelectedPath = Settings.Default.keymaps_path;
                    }
                    else // If the saved path is a file, try to get the parent directory
                    {
                        string initialDirectory = System.IO.Path.GetDirectoryName(Settings.Default.keymaps_path);
                        if (System.IO.Directory.Exists(initialDirectory))
                        {
                            folderBrowserDialog.SelectedPath = initialDirectory;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting initial directory for KeymapsPath: {ex.Message}");
                }
            }

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                // Ensures the path ends with the directory separator
                if (!selectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) && !selectedPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                {
                    selectedPath += System.IO.Path.DirectorySeparatorChar;
                }
                Settings.Default.keymaps_path = selectedPath;
                // Not saved here, it's saved with the Save & Restart button
            }
        }

        /// <summary>
        /// Handles the Click event for the Keymaps configuration file browse button.
        /// Opens an OpenFileDialog for the user to select the Keymaps.json file.
        /// </summary>
        private void BtnBrowseKeymapsConfig_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = KeymapsConfigSelect
            };

            // Tries to set the initial directory based on the Keymaps path or the current file path.
            string initialDirectory = Settings.Default.keymaps_path;
            if (!string.IsNullOrEmpty(Settings.Default.keymaps_config) && System.IO.File.Exists(System.IO.Path.Combine(Settings.Default.keymaps_path, Settings.Default.keymaps_config)))
            {
                openFileDialog.FileName = Settings.Default.keymaps_config; // Sets the default file name
            }

            try
            {
                if (System.IO.Directory.Exists(initialDirectory))
                {
                    openFileDialog.InitialDirectory = initialDirectory;
                }
                else if (!string.IsNullOrEmpty(Settings.Default.keymaps_config) && System.IO.File.Exists(Settings.Default.keymaps_config))
                {
                    // If keymaps_config is a valid absolute path, use its directory
                    openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(Settings.Default.keymaps_config);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting initial directory for KeymapsConfig: {ex.Message}");
            }

            if (openFileDialog.ShowDialog() == true)
            {
                // Stores only the file name, not the full path
                Settings.Default.keymaps_config = System.IO.Path.GetFileName(openFileDialog.FileName);
                // Not saved here, it's saved with the Save & Restart button
            }
        }

        /// <summary>
        /// Handles Checked/Unchecked events for CheckBox controls.
        /// Changes are automatically propagated to the DataContext thanks to UpdateSourceTrigger=PropertyChanged.
        /// </summary>
        private void Setting_Changed(object sender, RoutedEventArgs e)
        {
            // Changes are only saved with the Save & Restart button.
            // The DataContext is already automatically updated by the binding with UpdateSourceTrigger=PropertyChanged.
        }

        /// <summary>
        /// Handles the LostFocus event for TextBox controls.
        /// Changes are automatically propagated to the DataContext thanks to UpdateSourceTrigger=PropertyChanged.
        /// </summary>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Changes are only saved with the Save & Restart button.
            // The DataContext is already automatically updated by the binding with UpdateSourceTrigger=PropertyChanged.
        }

        /// <summary>
        /// Handles the ValueChanged event for Slider controls.
        /// Changes are automatically propagated to the DataContext thanks to UpdateSourceTrigger=PropertyChanged.
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Changes are only saved with the Save & Restart button.
            // The DataContext is already automatically updated by the binding with UpdateSourceTrigger=PropertyChanged.
        }

        /// <summary>
        /// Handles the LostFocus event for color component (R, G, B) text boxes.
        /// Parses the text and updates the corresponding color array in Settings.Default.
        /// </summary>
        private void ColorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            string tag = textBox.Tag?.ToString();
            if (string.IsNullOrEmpty(tag)) return;

            if (int.TryParse(textBox.Text, out int value))
            {
                value = Math.Max(0, Math.Min(255, value));
                textBox.Text = value.ToString();

                // Access Settings.Default directly
                Settings currentSettings = Settings.Default;

                Match match = Regex.Match(tag, @"ColorID(\d)_(\d)");
                if (match.Success)
                {
                    int colorId = int.Parse(match.Groups[1].Value);
                    int componentIndex = int.Parse(match.Groups[2].Value);

                    int[] targetColorArray = null; // Now it's an int array

                    switch (colorId)
                    {
                        case 1: targetColorArray = currentSettings.Color_ID1; break;
                        case 2: targetColorArray = currentSettings.Color_ID2; break;
                        case 3: targetColorArray = currentSettings.Color_ID3; break;
                        case 4: targetColorArray = currentSettings.Color_ID4; break;
                    }

                    if (targetColorArray != null && componentIndex >= 0 && componentIndex < targetColorArray.Length)
                    {
                        targetColorArray[componentIndex] = value;
                        // Notify that the array property has changed if necessary,
                        // although WPF bindings for array elements don't always capture it automatically.
                        // Since saving is manual with the button, the object in memory will already be updated.
                        // If UI notification is required, OnPropertyChanged("Color_IDx") could be called
                        // in the Settings class, but the Color_ID property in Settings.cs already does this.
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Invalid input for color component: {textBox.Text}");
            }
            // Changes are only saved with the Save & Restart button.
        }

        /// <summary>
        /// Prevents non-numeric input in text boxes intended for integer values.
        /// Allows only digits and control characters (like backspace).
        /// </summary>
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        /// <summary>
        /// Prevents non-numeric input (except a single decimal point) in text boxes intended for double values.
        /// </summary>
        private void DoubleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            // Allows digits and a single decimal separator based on the current culture
            bool isDigit = char.IsDigit(e.Text, 0);
            bool isDecimal = e.Text == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            // If it's a decimal separator, ensure there's only one in the text box
            if (isDecimal && textBox.Text.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
            {
                e.Handled = true;
            }
            // If it's not a digit and not a decimal separator, handle it (prevent input)
            else if (!isDigit && !isDecimal)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Overrides the OnClosing method to hide the window instead of closing it,
        /// allowing the instance to be reused.
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true; // Cancels the close event
            this.Hide();     // Hides the window
            base.OnClosing(e);
        }
    }
}
