using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Timers;
using WiiTUIO.DeviceUtils;
using WiiTUIO.Properties;
using PointF = WiimoteLib.PointF;
using System.Diagnostics;
using WiiTUIO.Filters;

namespace WiiTUIO.Provider
{
    /// <summary>
    /// Interaction logic for CalibrationOverlay.xaml
    /// </summary>
    public partial class CalibrationOverlay : Window
    {
        private WiiKeyMapper keyMapper;
        private static CalibrationOverlay defaultInstance;

        private System.Windows.Forms.Screen primaryScreen;
        private IntPtr previousForegroundWindow = IntPtr.Zero;

        private Timer buttonTimer;

        private bool hidden = true;
        private bool timerElapsed = false;

        private int step = 0;

        private float topOffset;
        private float bottomOffset;
        private float leftOffset;
        private float rightOffset;

        private float topBackup;
        private float bottomBackup;
        private float leftBackup;
        private float rightBackup;
        private float tlBackup;
        private float trBackup;
        private float centerXBackup;
        private float centerYBackup;

        private double marginXBackup;
        private double marginYBackup;

        /// <summary>
        /// An event which is raised once calibration is finished.
        /// </summary>
        public event Action OnCalibrationFinished;

        public static CalibrationOverlay Current
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new CalibrationOverlay();
                }
                return defaultInstance;
            }
        }

        public CalibrationOverlay()
        {
            InitializeComponent();

            primaryScreen = DeviceUtil.GetScreen(Settings.Default.primaryMonitor);

            Settings.Default.PropertyChanged += SettingsChanged;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            this.CalibrationCanvas.Visibility = Visibility.Hidden;

            buttonTimer = new Timer();
            buttonTimer.Interval = 1000;
            buttonTimer.AutoReset = true;
            buttonTimer.Elapsed += buttonTimer_Elapsed;

            //Compensate for DPI settings

            Loaded += (o, e) =>
            {
                this.updateWindowToScreen(primaryScreen);

                //Prevent OverlayWindow from showing up in alt+tab menu.
                UIHelpers.HideFromAltTab(this);
            };
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            this.primaryScreen = DeviceUtils.DeviceUtil.GetScreen(Settings.Default.primaryMonitor);
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.updateWindowToScreen(primaryScreen);
            }));
        }

        private void updateWindowToScreen(System.Windows.Forms.Screen screen)
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            Matrix transformMatrix = source.CompositionTarget.TransformToDevice;

            this.Width = screen.Bounds.Width * transformMatrix.M22;
            this.Height = screen.Bounds.Height * transformMatrix.M11;
            UIHelpers.SetWindowPos((new WindowInteropHelper(this)).Handle, IntPtr.Zero, screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height, UIHelpers.SetWindowPosFlags.SWP_NOACTIVATE | UIHelpers.SetWindowPosFlags.SWP_NOZORDER);
            this.CalibrationCanvas.Width = this.Width;
            this.CalibrationCanvas.Height = this.Height;
            UIHelpers.TopmostFix(this);
        }

        private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "primaryMonitor")
            {
                primaryScreen = DeviceUtil.GetScreen(Settings.Default.primaryMonitor);
                Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    this.updateWindowToScreen(primaryScreen);
                }));
            }
        }

        public void StartCalibration(WiiKeyMapper keyMapper)
        {
            if (this.hidden)
            {
                this.hidden = false;

                this.keyMapper = keyMapper;
                this.keyMapper.SwitchToCalibration();
                this.keyMapper.OnButtonDown += keyMapper_OnButtonDown;
                this.keyMapper.OnButtonUp += keyMapper_OnButtonUp;
                buttonTimer.Elapsed += buttonTimer_Elapsed;

                previousForegroundWindow = UIHelpers.GetForegroundWindow();
                if (previousForegroundWindow == null)
                {
                    previousForegroundWindow = IntPtr.Zero;
                }

                Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    this.Activate();

                    Color pointColor = IDColor.getColor(keyMapper.WiimoteID);
                    pointColor.R = (byte)(pointColor.R * 0.8);
                    pointColor.G = (byte)(pointColor.G * 0.8);
                    pointColor.B = (byte)(pointColor.B * 0.8);
                    SolidColorBrush brush = new SolidColorBrush(pointColor);

                    this.wiimoteNo.Text = "Wiimote " + keyMapper.WiimoteID;
                    this.wiimoteNo.Foreground = brush;

                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));

                    this.CalibrationCanvas.Opacity = 0.0;
                    this.CalibrationCanvas.Visibility = Visibility.Visible;

                    this.elipse.Stroke = this.lineX.Stroke = this.lineY.Stroke = brush;
                    this.elipse.Fill = new SolidColorBrush(Colors.Black);
                    this.elipse.Fill.Opacity = 0.9;

                    DoubleAnimation animation = UIHelpers.createDoubleAnimation(1.0, 200, false);
                    animation.FillBehavior = FillBehavior.HoldEnd;
                    animation.Completed += delegate (object sender, EventArgs pEvent)
                    {

                    };
                    this.CalibrationCanvas.BeginAnimation(FrameworkElement.OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
                }), null);

                marginXBackup = Settings.Default.CalibrationMarginX;
                marginYBackup = Settings.Default.CalibrationMarginY;

                topBackup = this.keyMapper.settings.Top;
                bottomBackup = this.keyMapper.settings.Bottom;
                leftBackup = this.keyMapper.settings.Left;
                rightBackup = this.keyMapper.settings.Right;

                if (Settings.Default.pointer_4IRMode == "none")
                {
                    this.movePoint(1 - marginXBackup, 1 - marginYBackup);

                    step = 1;
                }
                else
                {
                    Settings.Default.CalibrationMarginX = 0;
                    Settings.Default.CalibrationMarginY = 0;

                    centerXBackup = this.keyMapper.settings.CenterX;
                    centerYBackup = this.keyMapper.settings.CenterY;
                    tlBackup = this.keyMapper.settings.TLled;
                    trBackup = this.keyMapper.settings.TRled;

                    this.keyMapper.settings.Top = 0;
                    this.keyMapper.settings.Bottom = 1;
                    this.keyMapper.settings.Left = 0;
                    this.keyMapper.settings.Right = 1;

                    topOffset = 0;
                    bottomOffset = 1;
                    leftOffset = 0;
                    rightOffset = 1;

                    this.movePoint(0.5, 0.5);

                    step = 0;
                }
            }
        }

        void OverlayWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (!this.hidden)
            {
                if (e.Key == Key.Escape)
                {
                    HideOverlay();
                }
            }
        }

        private void HideOverlay()
        {
            if (!this.hidden)
            {
                this.hidden = true;
                this.timerElapsed = false;

                this.keyMapper.OnButtonUp -= keyMapper_OnButtonUp;
                this.keyMapper.OnButtonDown -= keyMapper_OnButtonDown;
                this.keyMapper.SwitchToFallback();
                buttonTimer.Elapsed -= buttonTimer_Elapsed;

                Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    if (previousForegroundWindow != IntPtr.Zero)
                    {
                        UIHelpers.SetForegroundWindow(previousForegroundWindow);
                    }
                    DoubleAnimation animation = UIHelpers.createDoubleAnimation(0.0, 200, false);
                    animation.FillBehavior = FillBehavior.HoldEnd;
                    animation.Completed += delegate (object sender, EventArgs pEvent)
                    {
                        this.CalibrationCanvas.Visibility = Visibility.Hidden;

                    };
                    this.CalibrationCanvas.BeginAnimation(FrameworkElement.OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
                }), null);
                step = 0;
            }
        }

        private void finishedCalibration()
        {
            if (Settings.Default.pointer_4IRMode != "none")
            {
                Settings.Default.Save();
            }

            this.keyMapper.settings.SaveCalibrationData();

            this.HideOverlay();
        }

        public void CancelCalibration()
        {
            this.keyMapper.settings.Top = topBackup;
            this.keyMapper.settings.Bottom = bottomBackup;
            this.keyMapper.settings.Left = leftBackup;
            this.keyMapper.settings.Right = rightBackup;

            if (Settings.Default.pointer_4IRMode != "none")
            {
                this.keyMapper.settings.CenterX = centerXBackup;
                this.keyMapper.settings.CenterY = centerYBackup;
                this.keyMapper.settings.TLled = tlBackup;
                this.keyMapper.settings.TRled = trBackup;

                Settings.Default.CalibrationMarginX = marginXBackup;
                Settings.Default.CalibrationMarginY = marginYBackup;

                Settings.Default.Save();
            }

            this.keyMapper.settings.SaveCalibrationData();

            this.HideOverlay();
        }

        private void keyMapper_OnButtonUp(WiiButtonEvent e)
        {
            e.Button = e.Button.Replace("OffScreen.", "");
            if (e.Button.ToLower().Equals("a") || e.Button.ToLower().Equals("b"))
            {
                this.buttonTimer.Stop();

                Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    this.wiimoteNo.Text = "Wiimote " + keyMapper.WiimoteID + ":";
                    this.insText2.Text = " aim at the targets and press A or B to calibrate";

                    this.TextBorder.UpdateLayout();
                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                }), null);

                if (this.timerElapsed)
                {
                    switch (step)
                    {
                        case 0:
                            this.movePoint(1 - marginXBackup, 1 - marginYBackup);

                            step = 1;
                            break;
                        case 1:
                            this.movePoint(marginXBackup, marginYBackup);

                            step = 2;
                            break;

                        case 2:
                            if (Settings.Default.pointer_4IRMode != "none")
                            {
                                this.keyMapper.settings.Top = topOffset;
                                this.keyMapper.settings.Bottom = bottomOffset;
                                this.keyMapper.settings.Left = leftOffset;
                                this.keyMapper.settings.Right = rightOffset;

                                Settings.Default.CalibrationMarginX = marginXBackup;
                                Settings.Default.CalibrationMarginY = marginYBackup;
                            }

                            Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                this.CalibrationPoint.Visibility = Visibility.Hidden;

                                this.wiimoteNo.Text = null;
                                this.insText2.Text = "Press A confirm calibration, press B to restart calibration";

                                this.TextBorder.UpdateLayout();
                                this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                            }), null);

                            step = 3;
                            break;

                        default: break;
                    }
                }

                this.timerElapsed = false;
            }
        }

        private void keyMapper_OnButtonDown(WiiButtonEvent e)
        {
            e.Button = e.Button.Replace("OffScreen.", "");
            if (step == 3)
            {
                if (e.Button.ToLower().Equals("a"))
                {
                    finishedCalibration();
                }
                else if (e.Button.ToLower().Equals("b"))
                {
                     if (Settings.Default.pointer_4IRMode == "none")
                    {
                        this.movePoint(1 - marginXBackup, 1 - marginYBackup);
                        step = 1;
                    }
                    else
                    {
                        this.movePoint(0.5, 0.5);
                        step = 0;
                    }
                }
            }
            else if (e.Button.ToLower().Equals("a") || e.Button.ToLower().Equals("b"))
            {
                if (!this.keyMapper.cursorPos.OutOfReach)
                {
                    this.buttonTimer.Start();
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.wiimoteNo.Text = null;
                        this.insText2.Text = "Hold";

                        this.TextBorder.UpdateLayout();
                        this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                        this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                    }), null);
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.wiimoteNo.Text = null;
                        this.insText2.Text = "Can't find sensors. Make sure you're at a proper distance and pointing at the screen";

                        this.TextBorder.UpdateLayout();
                        this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                        this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                    }), null);
                }
            }
        }

        void buttonTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.buttonTimer.Stop();
            this.timerElapsed = true;

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.wiimoteNo.Text = null;
                this.insText2.Text = "Release";

                this.TextBorder.UpdateLayout();
                this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
            }), null);

            switch (step)
            {
                case 0:
                    this.keyMapper.settings.CenterX = (float)((this.keyMapper.cursorPos.RelativeX - 2) * Math.Cos(this.keyMapper.cursorPos.Rotation) - (this.keyMapper.cursorPos.RelativeY - 2) * Math.Sin(this.keyMapper.cursorPos.Rotation) + 2);
                    this.keyMapper.settings.CenterY = (float)((this.keyMapper.cursorPos.RelativeX - 2) * Math.Sin(-this.keyMapper.cursorPos.Rotation) + (this.keyMapper.cursorPos.RelativeY - 2) * Math.Cos(-this.keyMapper.cursorPos.Rotation) + 2);
                    this.keyMapper.settings.TLled = (float)(0.5 - ((this.keyMapper.cursorPos.Width / this.keyMapper.cursorPos.Height) / 4));
                    this.keyMapper.settings.TRled = (float)(0.5 + ((this.keyMapper.cursorPos.Width / this.keyMapper.cursorPos.Height) / 4));
                    break;
                case 1:
                    if (Settings.Default.pointer_4IRMode == "none")
                    {
                        this.keyMapper.settings.Bottom = (float)this.keyMapper.cursorPos.RelativeY;
                        this.keyMapper.settings.Right = (float)this.keyMapper.cursorPos.RelativeX;
                    }
                    else
                    {
                        bottomOffset = (float)this.keyMapper.cursorPos.LightbarY;
                        rightOffset = (float)this.keyMapper.cursorPos.LightbarX;
                    }
                    break;
                case 2:
                    if (Settings.Default.pointer_4IRMode == "none")
                    {
                        this.keyMapper.settings.Top = (float)this.keyMapper.cursorPos.RelativeY;
                        this.keyMapper.settings.Left = (float)this.keyMapper.cursorPos.RelativeX;
                    }
                    else
                    {
                        topOffset = (float)this.keyMapper.cursorPos.LightbarY;
                        leftOffset = (float)this.keyMapper.cursorPos.LightbarX;
                    }
                    break;
                default: break;
            }
        }

        private Point movePoint(double fNormalX, double fNormalY)
        {
            Point tPoint = new Point(fNormalX * this.ActualWidth, fNormalY * this.ActualHeight);

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.CalibrationPoint.Visibility = Visibility.Visible;

                this.CalibrationPoint.SetValue(Canvas.LeftProperty, tPoint.X - (this.CalibrationPoint.ActualWidth / 2));
                this.CalibrationPoint.SetValue(Canvas.TopProperty, tPoint.Y - (this.CalibrationPoint.ActualHeight / 2));

            }), null);

            return tPoint;
        }

        public bool OverlayIsOn()
        {
            return !this.hidden;
        }
    }
}
