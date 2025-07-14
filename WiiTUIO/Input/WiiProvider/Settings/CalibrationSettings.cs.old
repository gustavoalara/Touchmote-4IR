using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WiiTUIO.Properties;

namespace WiiTUIO.Provider
{
    public class CalibrationSettings
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

        private float _Right = Settings.Default.pointer_4IRMode != "none" ? 0.9f : 0.1f;
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
                OnPropertyChanged("Center");
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
                OnPropertyChanged("Center");
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

        private static string CALIBRATION_FILENAME = System.AppDomain.CurrentDomain.BaseDirectory + "CalibrationData.json";
        private static JObject _calData;
        private List<string> propertyList;
        private string _id;

        public CalibrationSettings(string id)
        {
            _id = id;

            propertyList = Settings.Default.pointer_4IRMode != "none"
                ? new List<string> { "TRled", "TLled", "CenterX", "CenterY", "Right", "Left", "Bottom", "Top" }
                : new List<string> { "Right", "Left", "Bottom", "Top" };

            InitializeCalibrationData();
        }

        private void InitializeCalibrationData()
        {
            if (File.Exists(CALIBRATION_FILENAME))
            {
                string calText = File.ReadAllText(CALIBRATION_FILENAME);

                _calData = !string.IsNullOrEmpty(calText)
                    ? JObject.Parse(File.ReadAllText(CALIBRATION_FILENAME))
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
            File.WriteAllText(CALIBRATION_FILENAME, _calData.ToString());
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
