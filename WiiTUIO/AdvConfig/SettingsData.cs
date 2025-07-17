using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json; 

namespace WiiTUIO
{
    /// <summary>
    /// Represents the application settings data, mirroring the structure of settings.json.
    /// Implements INotifyPropertyChanged for WPF data binding.
    /// </summary>
    public class SettingsData : INotifyPropertyChanged
    {
        // Event for property change notifications (required by INotifyPropertyChanged)
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper method to raise the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- General Settings ---
        private bool _pairOnStart;
        [JsonProperty("pairOnStart")] // Changed to Newtonsoft.Json's JsonProperty
        public bool PairOnStart
        {
            get => _pairOnStart;
            set
            {
                if (_pairOnStart != value)
                {
                    _pairOnStart = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _connectOnStart;
        [JsonProperty("connectOnStart")] // Changed to Newtonsoft.Json's JsonProperty
        public bool ConnectOnStart
        {
            get => _connectOnStart;
            set
            {
                if (_connectOnStart != value)
                {
                    _connectOnStart = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _minimizeOnStart;
        [JsonProperty("minimizeOnStart")] // Changed to Newtonsoft.Json's JsonProperty
        public bool MinimizeOnStart
        {
            get => _minimizeOnStart;
            set
            {
                if (_minimizeOnStart != value)
                {
                    _minimizeOnStart = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _minimizeToTray;
        [JsonProperty("minimizeToTray")] // Changed to Newtonsoft.Json's JsonProperty
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                if (_minimizeToTray != value)
                {
                    _minimizeToTray = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _notificationsEnabled;
        [JsonProperty("notifications_enabled")] // Changed to Newtonsoft.Json's JsonProperty
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                if (_notificationsEnabled != value)
                {
                    _notificationsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _pairedOnce;
        [JsonProperty("pairedOnce")] // Changed to Newtonsoft.Json's JsonProperty
        public bool PairedOnce
        {
            get => _pairedOnce;
            set
            {
                if (_pairedOnce != value)
                {
                    _pairedOnce = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _primaryMonitor;
        [JsonProperty("primaryMonitor")] // Changed to Newtonsoft.Json's JsonProperty
        public string PrimaryMonitor
        {
            get => _primaryMonitor;
            set
            {
                if (_primaryMonitor != value)
                {
                    _primaryMonitor = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _completelyDisconnect;
        [JsonProperty("completelyDisconnect")] // Changed to Newtonsoft.Json's JsonProperty
        public bool CompletelyDisconnect
        {
            get => _completelyDisconnect;
            set
            {
                if (_completelyDisconnect != value)
                {
                    _completelyDisconnect = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _autoDisconnectTimeout;
        [JsonProperty("autoDisconnectTimeout")] // Changed to Newtonsoft.Json's JsonProperty
        public int AutoDisconnectTimeout
        {
            get => _autoDisconnectTimeout;
            set
            {
                if (_autoDisconnectTimeout != value)
                {
                    _autoDisconnectTimeout = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _defaultContinousScale;
        [JsonProperty("defaultContinousScale")] // Changed to Newtonsoft.Json's JsonProperty
        public double DefaultContinousScale
        {
            get => _defaultContinousScale;
            set
            {
                if (_defaultContinousScale != value)
                {
                    _defaultContinousScale = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _defaultContinousPressThreshold;
        [JsonProperty("defaultContinousPressThreshold")] // Changed to Newtonsoft.Json's JsonProperty
        public double DefaultContinousPressThreshold
        {
            get => _defaultContinousPressThreshold;
            set
            {
                if (_defaultContinousPressThreshold != value)
                {
                    _defaultContinousPressThreshold = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _defaultContinousDeadzone;
        [JsonProperty("defaultContinousDeadzone")] // Changed to Newtonsoft.Json's JsonProperty
        public double DefaultContinousDeadzone
        {
            get => _defaultContinousDeadzone;
            set
            {
                if (_defaultContinousDeadzone != value)
                {
                    _defaultContinousDeadzone = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _disconnectWiimotesOnDolphin;
        [JsonProperty("disconnectWiimotesOnDolphin")] // Changed to Newtonsoft.Json's JsonProperty
        public bool DisconnectWiimotesOnDolphin
        {
            get => _disconnectWiimotesOnDolphin;
            set
            {
                if (_disconnectWiimotesOnDolphin != value)
                {
                    _disconnectWiimotesOnDolphin = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dolphinPath;
        [JsonProperty("dolphin_path")] // Changed to Newtonsoft.Json's JsonProperty
        public string DolphinPath
        {
            get => _dolphinPath;
            set
            {
                if (_dolphinPath != value)
                {
                    _dolphinPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _keymapsPath;
        [JsonProperty("keymaps_path")] // Changed to Newtonsoft.Json's JsonProperty
        public string KeymapsPath
        {
            get => _keymapsPath;
            set
            {
                if (_keymapsPath != value)
                {
                    _keymapsPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _noTopmost;
        [JsonProperty("noTopmost")] // Changed to Newtonsoft.Json's JsonProperty
        public bool NoTopmost
        {
            get => _noTopmost;
            set
            {
                if (_noTopmost != value)
                {
                    _noTopmost = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _keymapsConfig;
        [JsonProperty("keymaps_config")] // Changed to Newtonsoft.Json's JsonProperty
        public string KeymapsConfig
        {
            get => _keymapsConfig;
            set
            {
                if (_keymapsConfig != value)
                {
                    _keymapsConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- Pointer Settings ---
        private double _pointerSensorBarPosCompensation;
        [JsonProperty("pointer_sensorBarPosCompensation")] // Changed to Newtonsoft.Json's JsonProperty
        public double PointerSensorBarPosCompensation
        {
            get => _pointerSensorBarPosCompensation;
            set
            {
                if (_pointerSensorBarPosCompensation != value)
                {
                    _pointerSensorBarPosCompensation = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _pointerCursorSize;
        [JsonProperty("pointer_cursorSize")] // Changed to Newtonsoft.Json's JsonProperty
        public double PointerCursorSize
        {
            get => _pointerCursorSize;
            set
            {
                if (_pointerCursorSize != value)
                {
                    _pointerCursorSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _pointerMarginsTopBottom;
        [JsonProperty("pointer_marginsTopBottom")] // Changed to Newtonsoft.Json's JsonProperty
        public double PointerMarginsTopBottom
        {
            get => _pointerMarginsTopBottom;
            set
            {
                if (_pointerMarginsTopBottom != value)
                {
                    _pointerMarginsTopBottom = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _pointerMarginsLeftRight;
        [JsonProperty("pointer_marginsLeftRight")] // Changed to Newtonsoft.Json's JsonProperty
        public double PointerMarginsLeftRight
        {
            get => _pointerMarginsLeftRight;
            set
            {
                if (_pointerMarginsLeftRight != value)
                {
                    _pointerMarginsLeftRight = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _pointerSensorBarPos;
        [JsonProperty("pointer_sensorBarPos")] // Changed to Newtonsoft.Json's JsonProperty
        public string PointerSensorBarPos
        {
            get => _pointerSensorBarPos;
            set
            {
                if (_pointerSensorBarPos != value)
                {
                    _pointerSensorBarPos = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _pointer4IRMode;
        [JsonProperty("pointer_4IRMode")] // Changed to Newtonsoft.Json's JsonProperty
        public string Pointer4IRMode
        {
            get => _pointer4IRMode;
            set
            {
                if (_pointer4IRMode != value)
                {
                    _pointer4IRMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _pointerFPS;
        [JsonProperty("pointer_FPS")] // Changed to Newtonsoft.Json's JsonProperty
        public int PointerFPS
        {
            get => _pointerFPS;
            set
            {
                if (_pointerFPS != value)
                {
                    _pointerFPS = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _pointerPositionSmoothing;
        [JsonProperty("pointer_positionSmoothing")] // Changed to Newtonsoft.Json's JsonProperty
        public int PointerPositionSmoothing
        {
            get => _pointerPositionSmoothing;
            set
            {
                if (_pointerPositionSmoothing != value)
                {
                    _pointerPositionSmoothing = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _pointerPositionRadius;
        [JsonProperty("pointer_positionRadius")] // Changed to Newtonsoft.Json's JsonProperty
        public double PointerPositionRadius
        {
            get => _pointerPositionRadius;
            set
            {
                if (_pointerPositionRadius != value)
                {
                    _pointerPositionRadius = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- Test Settings ---
        private bool _testDeltaAccel;
        [JsonProperty("test_deltaAccel")] // Changed to Newtonsoft.Json's JsonProperty
        public bool TestDeltaAccel
        {
            get => _testDeltaAccel;
            set
            {
                if (_testDeltaAccel != value)
                {
                    _testDeltaAccel = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testDeltaAccelMulti;
        [JsonProperty("test_deltaAccelMulti")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestDeltaAccelMulti
        {
            get => _testDeltaAccelMulti;
            set
            {
                if (_testDeltaAccelMulti != value)
                {
                    _testDeltaAccelMulti = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testDeltaAccelMinTravel;
        [JsonProperty("test_deltaAccelMinTravel")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestDeltaAccelMinTravel
        {
            get => _testDeltaAccelMinTravel;
            set
            {
                if (_testDeltaAccelMinTravel != value)
                {
                    _testDeltaAccelMinTravel = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testDeltaAccelEasingDuration;
        [JsonProperty("test_deltaAccelEasingDuration")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestDeltaAccelEasingDuration
        {
            get => _testDeltaAccelEasingDuration;
            set
            {
                if (_testDeltaAccelEasingDuration != value)
                {
                    _testDeltaAccelEasingDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testRegionEasingXDuration;
        [JsonProperty("test_regionEasingXDuration")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestRegionEasingXDuration
        {
            get => _testRegionEasingXDuration;
            set
            {
                if (_testRegionEasingXDuration != value)
                {
                    _testRegionEasingXDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testDeltaAccelMaxTravel;
        [JsonProperty("test_deltaAccelMaxTravel")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestDeltaAccelMaxTravel
        {
            get => _testDeltaAccelMaxTravel;
            set
            {
                if (_testDeltaAccelMaxTravel != value)
                {
                    _testDeltaAccelMaxTravel = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testRegionEasingXOffset;
        [JsonProperty("test_regionEasingXOffset")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestRegionEasingXOffset
        {
            get => _testRegionEasingXOffset;
            set
            {
                if (_testRegionEasingXOffset != value)
                {
                    _testRegionEasingXOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testSmoothingWeight;
        [JsonProperty("test_smoothingWeight")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestSmoothingWeight
        {
            get => _testSmoothingWeight;
            set
            {
                if (_testSmoothingWeight != value)
                {
                    _testSmoothingWeight = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testFpsmouseOffset;
        [JsonProperty("test_fpsmouseOffset")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestFpsmouseOffset
        {
            get => _testFpsmouseOffset;
            set
            {
                if (_testFpsmouseOffset != value)
                {
                    _testFpsmouseOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _calibrationMarginX;
        [JsonProperty("CalibrationMarginX")] // Changed to Newtonsoft.Json's JsonProperty
        public double CalibrationMarginX
        {
            get => _calibrationMarginX;
            set
            {
                if (_calibrationMarginX != value)
                {
                    _calibrationMarginX = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _calibrationMarginY;
        [JsonProperty("CalibrationMarginY")] // Changed to Newtonsoft.Json's JsonProperty
        public double CalibrationMarginY
        {
            get => _calibrationMarginY;
            set
            {
                if (_calibrationMarginY != value)
                {
                    _calibrationMarginY = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testLightgunOneeuroMincutoff;
        [JsonProperty("test_lightgun_oneeuro_mincutoff")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestLightgunOneeuroMincutoff
        {
            get => _testLightgunOneeuroMincutoff;
            set
            {
                if (_testLightgunOneeuroMincutoff != value)
                {
                    _testLightgunOneeuroMincutoff = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _testLightgunOneeuroBeta;
        [JsonProperty("test_lightgun_oneeuro_beta")] // Changed to Newtonsoft.Json's JsonProperty
        public double TestLightgunOneeuroBeta
        {
            get => _testLightgunOneeuroBeta;
            set
            {
                if (_testLightgunOneeuroBeta != value)
                {
                    _testLightgunOneeuroBeta = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _fpsmouseDeadzone;
        [JsonProperty("fpsmouse_deadzone")] // Changed to Newtonsoft.Json's JsonProperty
        public double FpsmouseDeadzone
        {
            get => _fpsmouseDeadzone;
            set
            {
                if (_fpsmouseDeadzone != value)
                {
                    _fpsmouseDeadzone = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _fpsmouseSpeed;
        [JsonProperty("fpsmouse_speed")] // Changed to Newtonsoft.Json's JsonProperty
        public int FpsmouseSpeed
        {
            get => _fpsmouseSpeed;
            set
            {
                if (_fpsmouseSpeed != value)
                {
                    _fpsmouseSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- Shake Settings ---
        private double _shakeThreshold;
        [JsonProperty("shake_threshold")] // Changed to Newtonsoft.Json's JsonProperty
        public double ShakeThreshold
        {
            get => _shakeThreshold;
            set
            {
                if (_shakeThreshold != value)
                {
                    _shakeThreshold = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _shakeCount;
        [JsonProperty("shake_count")] // Changed to Newtonsoft.Json's JsonProperty
        public int ShakeCount
        {
            get => _shakeCount;
            set
            {
                if (_shakeCount != value)
                {
                    _shakeCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _shakeMaxTimeInBetween;
        [JsonProperty("shake_maxTimeInBetween")] // Changed to Newtonsoft.Json's JsonProperty
        public int ShakeMaxTimeInBetween
        {
            get => _shakeMaxTimeInBetween;
            set
            {
                if (_shakeMaxTimeInBetween != value)
                {
                    _shakeMaxTimeInBetween = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _shakePressedTime;
        [JsonProperty("shake_pressedTime")] // Changed to Newtonsoft.Json's JsonProperty
        public int ShakePressedTime
        {
            get => _shakePressedTime;
            set
            {
                if (_shakePressedTime != value)
                {
                    _shakePressedTime = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- XInput Rumble Settings ---
        private int _xinputRumbleThresholdBig;
        [JsonProperty("xinput_rumbleThreshold_big")] // Changed to Newtonsoft.Json's JsonProperty
        public int XinputRumbleThresholdBig
        {
            get => _xinputRumbleThresholdBig;
            set
            {
                if (_xinputRumbleThresholdBig != value)
                {
                    _xinputRumbleThresholdBig = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _xinputRumbleThresholdSmall;
        [JsonProperty("xinput_rumbleThreshold_small")] // Changed to Newtonsoft.Json's JsonProperty
        public int XinputRumbleThresholdSmall
        {
            get => _xinputRumbleThresholdSmall;
            set
            {
                if (_xinputRumbleThresholdSmall != value)
                {
                    _xinputRumbleThresholdSmall = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- Wiimode Rumble & Sound Settings ---
        private int _wiimodeRumbleTimeShort;
        [JsonProperty("wiimode_rumbleTime_short")] // Changed to Newtonsoft.Json's JsonProperty
        public int WiimodeRumbleTimeShort
        {
            get => _wiimodeRumbleTimeShort;
            set
            {
                if (_wiimodeRumbleTimeShort != value)
                {
                    _wiimodeRumbleTimeShort = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _wiimodeRumbleTimeLong;
        [JsonProperty("wiimode_rumbleTime_long")] // Changed to Newtonsoft.Json's JsonProperty
        public int WiimodeRumbleTimeLong
        {
            get => _wiimodeRumbleTimeLong;
            set
            {
                if (_wiimodeRumbleTimeLong != value)
                {
                    _wiimodeRumbleTimeLong = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _wiimodeRumbleTimeAlternatingOn;
        [JsonProperty("wiimode_rumbleTime_alternatingOn")] // Changed to Newtonsoft.Json's JsonProperty
        public int WiimodeRumbleTimeAlternatingOn
        {
            get => _wiimodeRumbleTimeAlternatingOn;
            set
            {
                if (_wiimodeRumbleTimeAlternatingOn != value)
                {
                    _wiimodeRumbleTimeAlternatingOn = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _wiimodeRumbleTimeAlternatingOff;
        [JsonProperty("wiimode_rumbleTime_alternatingOff")] // Changed to Newtonsoft.Json's JsonProperty
        public int WiimodeRumbleTimeAlternatingOff
        {
            get => _wiimodeRumbleTimeAlternatingOff;
            set
            {
                if (_wiimodeRumbleTimeAlternatingOff != value)
                {
                    _wiimodeRumbleTimeAlternatingOff = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _wiimodeLoopSoundTime;
        [JsonProperty("wiimode_loopSoundTime")] // Changed to Newtonsoft.Json's JsonProperty
        public int WiimodeLoopSoundTime
        {
            get => _wiimodeLoopSoundTime;
            set
            {
                if (_wiimodeLoopSoundTime != value)
                {
                    _wiimodeLoopSoundTime = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- Color IDs (represented as lists of integers) ---
        private List<int> _colorID1;
        [JsonProperty("Color_ID1")] // Changed to Newtonsoft.Json's JsonProperty
        public List<int> ColorID1
        {
            get => _colorID1;
            set
            {
                if (_colorID1 != value)
                {
                    _colorID1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<int> _colorID2;
        [JsonProperty("Color_ID2")] // Changed to Newtonsoft.Json's JsonProperty
        public List<int> ColorID2
        {
            get => _colorID2;
            set
            {
                if (_colorID2 != value)
                {
                    _colorID2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<int> _colorID3;
        [JsonProperty("Color_ID3")] // Changed to Newtonsoft.Json's JsonProperty
        public List<int> ColorID3
        {
            get => _colorID3;
            set
            {
                if (_colorID3 != value)
                {
                    _colorID3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<int> _colorID4;
        [JsonProperty("Color_ID4")] // Changed to Newtonsoft.Json's JsonProperty
        public List<int> ColorID4
        {
            get => _colorID4;
            set
            {
                if (_colorID4 != value)
                {
                    _colorID4 = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
