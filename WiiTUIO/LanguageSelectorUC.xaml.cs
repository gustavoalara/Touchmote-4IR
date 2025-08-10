using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WiiTUIO.Properties;

namespace WiiTUIO
{
    public partial class LanguageSelectorUC : UserControl
    {
        public event Action OnClose;
        private readonly Dictionary<string, string> _languageMap;
        private bool _languageChanged = false;

        public LanguageSelectorUC()
        {
            InitializeComponent();

            // Rellenamos el ComboBox
            var availableCultures = GetAvailableCultures();
            _languageMap = availableCultures.ToDictionary(
                c => c.NativeName.Substring(0, 1).ToUpper() + c.NativeName.Substring(1),
                c => c.Name
            );
            LanguageComboBox.ItemsSource = _languageMap.Keys;

            // Seleccionamos el idioma actual en la lista
            string currentCultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var currentLanguage = _languageMap.FirstOrDefault(x => x.Value == currentCultureName).Key;
            if (currentLanguage != null)
            {
                LanguageComboBox.SelectedItem = currentLanguage;
            }
            else if (_languageMap.Any())
            {
                LanguageComboBox.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem != null)
            {
                string selectedCultureCode = _languageMap[LanguageComboBox.SelectedItem.ToString()];
                // Solo guardamos y marcamos para reiniciar si el idioma es diferente
                if (selectedCultureCode != Settings.Default.DefaultLanguage)
                {
                    Settings.Default.DefaultLanguage = selectedCultureCode;
                    Settings.Default.Save();
                    _languageChanged = true;
                }
            }
            // Disparamos el evento OnClose, la MainWindow se encargará del resto
            OnClose?.Invoke();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _languageChanged = false;
            OnClose?.Invoke();
        }

        // Método para que la MainWindow sepa si debe reiniciar
        public bool ShouldRestart()
        {
            return _languageChanged;
        }

        // Este método lo hemos movido aquí desde MainWindow para que sea autocontenido
        private List<System.Globalization.CultureInfo> GetAvailableCultures()
        {
            var cultures = new List<System.Globalization.CultureInfo>();
            var exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (exePath == null) return cultures;

            cultures.Add(new System.Globalization.CultureInfo("en")); // Idioma por defecto

            foreach (var dir in System.IO.Directory.GetDirectories(exePath))
            {
                try
                {
                    var dirInfo = new System.IO.DirectoryInfo(dir);
                    var culture = System.Globalization.CultureInfo.GetCultureInfo(dirInfo.Name);
                    if (System.IO.File.Exists(System.IO.Path.Combine(dir, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.resources.dll")))
                    {
                        cultures.Add(culture);
                    }
                }
                catch { }
            }
            return cultures.Distinct().ToList();
        }
    }
}