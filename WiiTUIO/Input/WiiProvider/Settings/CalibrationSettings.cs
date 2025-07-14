using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WiiTUIO.Properties;

namespace WiiTUIO.Provider
{
    public class CalibrationSettings : INotifyPropertyChanged // Implementamos INotifyPropertyChanged explícitamente para mayor claridad
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private float _Top = 0.1f;
        public float Top
        {
            get => _Top;
            set
            {
                if (_Top == value) return;
                _Top = value;
                _calData[_id]["Top"] = JToken.FromObject(value);
                OnPropertyChanged("Top");
            }
        }

        private float _Bottom = 0.9f;
        public float Bottom
        {
            get => _Bottom;
            set
            {
                if (_Bottom == value) return;
                _Bottom = value;
                _calData[_id]["Bottom"] = JToken.FromObject(value);
                OnPropertyChanged("Bottom");
            }
        }

        private float _Left = Settings.Default.pointer_4IRMode != "none" ? 0.1f : 0.9f;
        public float Left
        {
            get => _Left;
            set
            {
                if (_Left == value) return;
                _Left = value;
                _calData[_id]["Left"] = JToken.FromObject(value);
                OnPropertyChanged("Left");
            }
        }

        private float _Right = Settings.Default.pointer_4IRMode != "none" ? 1f : 0f;
        public float Right
        {
            get => _Right;
            set
            {
                if (_Right == value) return;
                _Right = value;
                _calData[_id]["Right"] = JToken.FromObject(value);
                OnPropertyChanged("Right");
            }
        }

        private float _CenterX = 0.5f;
        public float CenterX
        {
            get => _CenterX;
            set
            {
                if (_CenterX == value) return;
                _CenterX = (float)Math.Min(1.0, Math.Max(0.0, value));
                _calData[_id]["CenterX"] = JToken.FromObject(value);
                OnPropertyChanged("Center"); // El evento se llama "Center" pero la propiedad es CenterX
            }
        }

        private float _CenterY = 0.5f;
        public float CenterY
        {
            get => _CenterY;
            set
            {
                if (_CenterY == value) return;
                _CenterY = (float)Math.Min(1.0, Math.Max(0.0, value));
                _calData[_id]["CenterY"] = JToken.FromObject(value);
                OnPropertyChanged("Center"); // El evento se llama "Center" pero la propiedad es CenterY
            }
        }

        private float _TLled = 0.26f;
        public float TLled
        {
            get => _TLled;
            set
            {
                if (_TLled == value) return;
                _TLled = value;
                _calData[_id]["TLled"] = JToken.FromObject(value);
                OnPropertyChanged("TLled");
            }
        }

        private float _TRled = 0.74f;
        public float TRled
        {
            get => _TRled;
            set
            {
                if (_TRled == value) return;
                _TRled = value;
                _calData[_id]["TRled"] = JToken.FromObject(value);
                OnPropertyChanged("TRled");
            }
        }

        // --- NUEVAS PROPIEDADES PARA EL MODO DIAMANTE ---
        
        private float _DiamondRightX = 1.0f; // Valor inicial ajustado
        public float DiamondRightX
        {
            get => _DiamondRightX;
            set
            {
                if (_DiamondRightX == value) return;
                _DiamondRightX =  value;
                _calData[_id]["DiamondRightX"] = JToken.FromObject(value);
                OnPropertyChanged("DiamondRightX");
            }
        }

        private float _DiamondBottomY = 0.0f; // Valor inicial ajustado
        public float DiamondBottomY
        {
            get => _DiamondBottomY;
            set
            {
                if (_DiamondBottomY == value) return;
                _DiamondBottomY = value;
                _calData[_id]["DiamondBottomY"] = JToken.FromObject(value);
                OnPropertyChanged("DiamondBottomY");
            }
        }

        private float _DiamondLeftX = 0.0f; // Valor inicial ajustado
        public float DiamondLeftX
        {
            get => _DiamondLeftX;
            set
            {
                if (_DiamondLeftX == value) return;
                _DiamondLeftX = value;
                _calData[_id]["DiamondLeftX"] = JToken.FromObject(value);
                OnPropertyChanged("DiamondLeftX");
            }
        }

        private float _DiamondTopY = 1.0f; // Valor inicial ajustado para dejar un pequeño margen
        public float DiamondTopY
        {
            get => _DiamondTopY;
            set
            {
                if (_DiamondTopY == value) return;
                _DiamondTopY = value;
                _calData[_id]["DiamondTopY"] = JToken.FromObject(value);
                OnPropertyChanged("DiamondTopY");
            }
        }
        // --- FIN DE NUEVAS PROPIEDADES ---

        private static string CALIBRATION_FILENAME = System.AppDomain.CurrentDomain.BaseDirectory + "CalibrationData.json";
        private static JObject _calData;
        private List<string> propertyList;
        private string _id;

        public CalibrationSettings(string id)
        {
            _id = id;

            // --- MODIFICACIÓN DE LA LISTA DE PROPIEDADES A GUARDAR/CARGAR ---
            // Aseguramos que las propiedades del diamante se incluyan cuando el modo no sea "none".
            propertyList = Settings.Default.pointer_4IRMode != "none"
                ? new List<string>
                {
                    "TRled", "TLled",        // Para el modo 'square'
                    "CenterX", "CenterY",    // Para el centro del Wiimote
                    "Right", "Left", "Bottom", "Top", // Para el modo 'none' (rectángulo base)

                    // ¡NUEVAS PROPIEDADES PARA EL MODO 'DIAMOND'!
                    "DiamondTopY", "DiamondBottomY", "DiamondLeftX", "DiamondRightX"
                }
                : new List<string> { "Right", "Left", "Bottom", "Top" }; // Mantener si "none" es el modo base

            InitializeCalibrationData();
        }

        private void InitializeCalibrationData()
        {
            if (File.Exists(CALIBRATION_FILENAME))
            {
                string calText = File.ReadAllText(CALIBRATION_FILENAME);

                _calData = !string.IsNullOrEmpty(calText)
                    ? JObject.Parse(calText) // Usamos calText directamente
                    : new JObject();
            }
            else
            {
                _calData = new JObject();
            }

            _calData[_id] = _calData[_id] ?? new JObject();

            LoadCalibrationValues();
            SaveCalibrationData();
        }

        private void LoadCalibrationValues()
        {
            foreach (var property in GetType().GetProperties())
            {
                if (propertyList.Contains(property.Name))
                {
                    if (_calData[_id][property.Name] != null)
                    {
                        var value = _calData[_id][property.Name].ToObject(property.PropertyType);
                        property.SetValue(this, value);
                    }
                    else
                    {
                        // Si la propiedad no está en el JSON, la inicializamos con su valor por defecto
                        // y la guardamos en el JSON para la próxima vez.
                        var value = property.GetValue(this);
                        if (value == null && property.PropertyType.IsValueType)
                        {
                            value = Activator.CreateInstance(property.PropertyType);
                        }
                        _calData[_id][property.Name] = JToken.FromObject(value);
                    }
                }
            }
        }

        public void SaveCalibrationData()
        {
            File.WriteAllText(CALIBRATION_FILENAME, _calData.ToString(Newtonsoft.Json.Formatting.Indented)); // Para formato más legible
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}