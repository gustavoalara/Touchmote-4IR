using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics; // Añadido para Debug.WriteLine

namespace WiiTUIO
{
    /// <summary>
    /// Manages loading and saving application settings from/to a JSON file.
    /// </summary>
    public static class SettingsManager
    {
        // Define la ruta del archivo de configuración en el mismo directorio que el ejecutable
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static SettingsData _currentSettings;

        /// <summary>
        /// Gets the current application settings. Loads them if not already loaded.
        /// </summary>
        public static SettingsData CurrentSettings
        {
            get
            {
                if (_currentSettings == null)
                {
                    LoadSettings();
                }
                return _currentSettings;
            }
        }

        /// <summary>
        /// Loads settings from the settings.json file. If the file doesn't exist or is invalid,
        /// a new default SettingsData object is created.
        /// </summary>
        public static void LoadSettings()
        {
            Debug.WriteLine($"Attempting to load settings from: {SettingsFilePath}");
            try
            {
                // No es necesario crear el directorio base de la aplicación, ya existe.

                if (File.Exists(SettingsFilePath))
                {
                    string jsonString = File.ReadAllText(SettingsFilePath);
                    _currentSettings = JsonConvert.DeserializeObject<SettingsData>(jsonString);
                    Debug.WriteLine("Settings loaded successfully.");
                }
                else
                {
                    Debug.WriteLine("settings.json not found. Creating default settings.");
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings(); // Save the default settings
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}. StackTrace: {ex.StackTrace}. Creating default settings.");
                _currentSettings = CreateDefaultSettings();
                SaveSettings(); // Save default settings in case of load error
            }
        }

        /// <summary>
        /// Saves the current settings to the settings.json file.
        /// </summary>
        public static void SaveSettings()
        {
            Debug.WriteLine($"Attempting to save settings to: {SettingsFilePath}");
            try
            {
                if (_currentSettings != null)
                {
                    string jsonString = JsonConvert.SerializeObject(_currentSettings, Formatting.Indented);
                    File.WriteAllText(SettingsFilePath, jsonString);
                    Debug.WriteLine("Settings saved successfully to settings.json.");
                }
                else
                {
                    Debug.WriteLine("Cannot save settings: _currentSettings is null.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}. StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Creates a default SettingsData object with values matching your provided JSON.
        /// </summary>
        /// <returns>A new SettingsData object with default values.</returns>
        private static SettingsData CreateDefaultSettings()
        {
            Debug.WriteLine("Creating default settings...");
            return new SettingsData
            {
                PairOnStart = false,
                ConnectOnStart = true,
                MinimizeOnStart = false,
                MinimizeToTray = false,
                NotificationsEnabled = true,
                PairedOnce = false,
                PrimaryMonitor = "",
                CompletelyDisconnect = false,
                AutoDisconnectTimeout = 300000,
                DefaultContinousScale = 1.0,
                DefaultContinousPressThreshold = 0.4,
                DefaultContinousDeadzone = 0.01,
                DisconnectWiimotesOnDolphin = false,
                DolphinPath = "",
                KeymapsPath = "Keymaps\\",
                NoTopmost = false,
                KeymapsConfig = "Keymaps.json",
                PointerSensorBarPosCompensation = 0.3,
                PointerCursorSize = 0.03,
                PointerMarginsTopBottom = 0.5,
                PointerMarginsLeftRight = 0.4,
                PointerSensorBarPos = "center",
                Pointer4IRMode = "diamond",
                PointerFPS = 100,
                PointerPositionSmoothing = 2,
                PointerPositionRadius = 0.002,
                TestDeltaAccel = true,
                TestDeltaAccelMulti = 4.0,
                TestDeltaAccelMinTravel = 0.02,
                TestDeltaAccelEasingDuration = 0.2,
                TestRegionEasingXDuration = 0.1,
                TestDeltaAccelMaxTravel = 0.425,
                TestRegionEasingXOffset = 0.8,
                TestSmoothingWeight = 0.25,
                TestFpsmouseOffset = 0.8,
                CalibrationMarginX = 0.0,
                CalibrationMarginY = 0.0,
                TestLightgunOneeuroMincutoff = 2.0,
                TestLightgunOneeuroBeta = 0.92,
                FpsmouseDeadzone = 0.021,
                FpsmouseSpeed = 35,
                ShakeThreshold = 0.2,
                ShakeCount = 2,
                ShakeMaxTimeInBetween = 500,
                ShakePressedTime = 200,
                XinputRumbleThresholdBig = 200,
                XinputRumbleThresholdSmall = 200,
                WiimodeRumbleTimeShort = 200,
                WiimodeRumbleTimeLong = 500,
                WiimodeRumbleTimeAlternatingOn = 100,
                WiimodeRumbleTimeAlternatingOff = 100,
                WiimodeLoopSoundTime = 200,
                ColorID1 = new List<int> { 128, 255, 0 },
                ColorID2 = new List<int> { 197, 0, 255 },
                ColorID3 = new List<int> { 0, 220, 255 },
                ColorID4 = new List<int> { 255, 255, 0 }
            };
        }
    }
}
