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
using WiimoteLib;
using static System.Windows.Forms.AxHost;
using System.Windows.Media.Media3D;

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

        private int step = 0; // Paso actual de la calibración

        private float topOffset;
        private float bottomOffset;
        private float leftOffset;
        private float rightOffset;

        // Propiedades para hacer un backup de los valores actuales antes de calibrar
        private float topBackup;
        private float bottomBackup;
        private float leftBackup;
        private float rightBackup;
        private float tlBackup;
        private float trBackup;
        private float centerXBackup;
        private float centerYBackup;

        // --- BACKUPS PARA EL MODO DIAMANTE ---
        private float diamondTopYBackup;
        private float diamondBottomYBackup;
        private float diamondLeftXBackup;
        private float diamondRightXBackup;
        // --- FIN BACKUPS PARA EL MODO DIAMANTE ---

        private double marginXBackup;
        private double marginYBackup;

        private int yMin, yMax, xMin, xMax;
        private PointF[] finalPos = new PointF[4];
        private uint[] see = new uint[4];

        Wiimote Wiimote;


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

                    this.wiimoteNo.Text = "Wiimote " + keyMapper.WiimoteID + " ";
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
                        // Animation completed, ready for first step
                    };
                    this.CalibrationCanvas.BeginAnimation(FrameworkElement.OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
                }), null);

                // --- BACKUP DE VALORES ACTUALES Y PREPARACIÓN PARA CADA MODO ---
                // Siempre hacemos backup de los settings básicos de top, bottom, left, right
                topBackup = this.keyMapper.settings.Top;
                bottomBackup = this.keyMapper.settings.Bottom;
                leftBackup = this.keyMapper.settings.Left;
                rightBackup = this.keyMapper.settings.Right;

                // Capturamos el backup de los márgenes aquí, ya que se usan en "none" y se restauran en "square" si se cancela.
                marginXBackup = Settings.Default.CalibrationMarginX;
                marginYBackup = Settings.Default.CalibrationMarginY;

                // BACKUP para modo SQUARE
                centerXBackup = this.keyMapper.settings.CenterX;
                centerYBackup = this.keyMapper.settings.CenterY;
                tlBackup = this.keyMapper.settings.TLled;
                trBackup = this.keyMapper.settings.TRled; // Corregido

                // BACKUP para modo DIAMANTE
                diamondTopYBackup = this.keyMapper.settings.DiamondTopY;
                diamondBottomYBackup = this.keyMapper.settings.DiamondBottomY;
                diamondLeftXBackup = this.keyMapper.settings.DiamondLeftX;
                diamondRightXBackup = this.keyMapper.settings.DiamondRightX;

                // Inicialización del primer paso según el modo
                if (Settings.Default.pointer_4IRMode == "none")
                {
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.movePoint(1 - marginXBackup, 1 - marginYBackup); // Esquina Inferior Derecha
                        this.insText2.Text = "Apunte a la esquina INFERIOR DERECHA y presione A o B para calibrar";
                        this.TextBorder.UpdateLayout();
                        this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                        this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                    }), null);

                    step = 1;
                }
                else if (Settings.Default.pointer_4IRMode == "square")
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
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.movePoint(0.5, 0.5); // Centro
                        this.insText2.Text = "Apunte al objetivo CENTRAL y presione A o B para calibrar";
                        this.TextBorder.UpdateLayout();
                        this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                        this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                    }), null);
                    step = 0; // Paso 0 para el centro
                }
                else if (Settings.Default.pointer_4IRMode == "diamond")
                {
                    Settings.Default.CalibrationMarginX = 0; // No usamos los márgenes estándar
                    Settings.Default.CalibrationMarginY = 0;

                    //this.keyMapper.settings.CenterX = 0.5f;
                    //this.keyMapper.settings.CenterY = 0.5f;

                    centerXBackup = this.keyMapper.settings.CenterX;
                    centerYBackup = this.keyMapper.settings.CenterY;

                    this.keyMapper.settings.Top = 0; // Restaurar valores predeterminados para la calibración
                    this.keyMapper.settings.Bottom = 1;
                    this.keyMapper.settings.Left = 0;
                    this.keyMapper.settings.Right = 1;

                    this.keyMapper.settings.DiamondTopY = 1.0f; // Valor por defecto
                    this.keyMapper.settings.DiamondBottomY = 0.0f; // Valor por defecto
                    this.keyMapper.settings.DiamondLeftX = 0.0f; // Valor por defecto
                    this.keyMapper.settings.DiamondRightX = 1.0f; // Valor por defecto
                    
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.movePoint(0.5, 0.5); // Centro
                        this.insText2.Text = "Apunte al objetivo CENTRAL y presione A o B para calibrar";
                        this.TextBorder.UpdateLayout();
                        this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                        this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                    }), null);
                    step = 0; // Paso 0 para el centro
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
                step = 0; // Reiniciar el contador de pasos al ocultar
            }
        }

        private void finishedCalibration()
        {
            // Solo guardamos Settings.Default si el modo no es "none", ya que solo 4IRMode las modifica
            if (Settings.Default.pointer_4IRMode != "none")
            {
                // Si square, guardamos los Settings para que persistan los cambios de CenterX, CenterY, TLled, TRled
                // y los márgenes de calibración restaurados.
                Settings.Default.Save();
            }
            // Los modos none y diamond no necesitan Settings.Default.Save() aquí
            // ya que sus propiedades relevantes se guardan en el WiimoteSettings.SaveCalibrationData()

            this.keyMapper.settings.SaveCalibrationData(); // Guarda la calibración del Wiimote

            this.HideOverlay();
        }

        public void CancelCalibration()
        {
            // Restaurar los valores de backup según el modo
            this.keyMapper.settings.Top = topBackup;
            this.keyMapper.settings.Bottom = bottomBackup;
            this.keyMapper.settings.Left = leftBackup;
            this.keyMapper.settings.Right = rightBackup;

            if (Settings.Default.pointer_4IRMode == "square")
            {
                this.keyMapper.settings.CenterX = centerXBackup;
                this.keyMapper.settings.CenterY = centerYBackup;
                this.keyMapper.settings.TLled = tlBackup;
                this.keyMapper.settings.TRled = trBackup; // Restaurar TRled también

                // Asegurarse de restaurar también los márgenes de calibración para el modo square
                // ya que se resetean a 0 al iniciar la calibración en square.
                Settings.Default.CalibrationMarginX = marginXBackup;
                Settings.Default.CalibrationMarginY = marginYBackup;

                Settings.Default.Save();


            }
            else if (Settings.Default.pointer_4IRMode == "diamond")
            {
				//this.keyMapper.settings.CenterX = 0.5f;
                //this.keyMapper.settings.CenterY = 0.5f;

                this.keyMapper.settings.CenterX = centerXBackup;
                this.keyMapper.settings.CenterY = centerYBackup;

                this.keyMapper.settings.DiamondTopY = diamondTopYBackup;
                this.keyMapper.settings.DiamondBottomY = diamondBottomYBackup;
                this.keyMapper.settings.DiamondLeftX = diamondLeftXBackup;
                this.keyMapper.settings.DiamondRightX = diamondRightXBackup;
            }

            

            this.keyMapper.settings.SaveCalibrationData(); // Guarda los valores restaurados

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
                    // Resetear la instrucción general
                    this.insText2.Text = " apunte a los objetivos y presione A o B para calibrar";

                    this.TextBorder.UpdateLayout();
                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                }), null);

                if (this.timerElapsed)
                {
                    // --- LÓGICA DE AVANCE DE PASOS DE CALIBRACIÓN CON IF ANIDADO POR MODO ---
                    switch (step)
                    {
                        case 0: 
                            if (Settings.Default.pointer_4IRMode == "square")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(1 - marginXBackup, 1 - marginYBackup); // Esquina Inferior Derecha
                                    this.insText2.Text = "Apunte a la esquina INFERIOR DERECHA y presione A o B para calibrar";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 1;
                            }
                            else if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(0.5, 0); // Arriba-Centro
                                    this.insText2.Text = "Apunte al objetivo SUPERIOR central y presione A o B para calibrar";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 1;
                            }
                            break;
                        case 1:
                            if (Settings.Default.pointer_4IRMode == "none" || Settings.Default.pointer_4IRMode == "square")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(marginXBackup, marginYBackup); 
                                    this.insText2.Text = "Apunte a la esquina SUPERIOR IZQUIERDA y presione A o B para calibrar";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 2;
                            }
                            else if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(0.5, 1); // Abajo-Centro
                                    this.insText2.Text = "Apunte al objetivo INFERIOR central y presione A o B para calibrar";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 2;
                            }
                            break;
                        case 2:
                            // Este es el último punto de calibración para "none" y "square"
                            if (Settings.Default.pointer_4IRMode == "none" || Settings.Default.pointer_4IRMode == "square")
                            {
                                // Lógica de asignación final para square (ya que Top, Bottom, Left, Right se asignan en buttonTimer_Elapsed)
                                if (Settings.Default.pointer_4IRMode == "square")
                                {
                                    this.keyMapper.settings.Top = topOffset;
                                    this.keyMapper.settings.Bottom = bottomOffset;
                                    this.keyMapper.settings.Left = leftOffset;
                                    this.keyMapper.settings.Right = rightOffset;
                                    // Los valores CenterX, CenterY, TLled y TRled se capturaron en el paso 0
                                    // Y Top, Bottom, Left, Right en pasos 1 y 2
                                    Settings.Default.CalibrationMarginX = marginXBackup; // Restaurar márgenes
                                    Settings.Default.CalibrationMarginY = marginYBackup;
                                }

                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.CalibrationPoint.Visibility = Visibility.Hidden; // Oculta el objetivo
                                    this.wiimoteNo.Text = null;
                                    this.insText2.Text = "Presione A para confirmar la calibración, presione B para reiniciar la calibración";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 5; // Ir directamente al paso de confirmación unificado
                            }
                            else if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(0, 0.5); // Izquierda-Centro
                                    this.insText2.Text = "Apunte al objetivo IZQUIERDO central y presione A o B para calibrar";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 3;
                            }
                            break;
                        case 3:
                            // Este paso solo se alcanza en "diamond" para derecha-centro
                            if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(1, 0.5); // Derecha-Centro
                                    this.insText2.Text = "Apunte al objetivo DERECHO central y presione A o B para calibrar";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 4;
                            }
                            break;
                        case 4:
                            // Este es el último punto de calibración para "diamond"
                            if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                // No se necesita lógica de asignación aquí, ya que DiamondTopY, BottomY, LeftX, RightX
                                // se asignan directamente en buttonTimer_Elapsed en sus respectivos pasos.
                                // Tampoco hay márgenes de calibración de Settings.Default que restaurar para este modo.

                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.CalibrationPoint.Visibility = Visibility.Hidden; // Oculta el objetivo
                                    this.wiimoteNo.Text = null;
                                    this.insText2.Text = "Presione A para confirmar la calibración, presione B para reiniciar la calibración";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 5; // Ir directamente al paso de confirmación unificado
                            }
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
            // Lógica de confirmación o reinicio de calibración
            bool isConfirmStep = (step == 5);

            if (isConfirmStep)
            {
                if (e.Button.ToLower().Equals("a"))
                {
                    finishedCalibration();
                }
                else if (e.Button.ToLower().Equals("b"))
                {
                    // Reiniciar la calibración según el modo
                    if (Settings.Default.pointer_4IRMode == "none")
                    {
                        Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            this.movePoint(1 - marginXBackup, 1 - marginYBackup); // Vuelve al primer punto del modo 'none'
                            this.insText2.Text = "Apunte a la esquina INFERIOR DERECHA y presione A o B para calibrar";
                            this.TextBorder.UpdateLayout();
                            this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                            this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                        }), null);
                        step = 1;
                    }
                    else if (Settings.Default.pointer_4IRMode == "square" || Settings.Default.pointer_4IRMode == "diamond")
                    {
                        Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            this.movePoint(0.5, 0.5); // Vuelve al centro
                            this.insText2.Text = "Apunte al objetivo CENTRAL y presione A o B para calibrar";
                            this.TextBorder.UpdateLayout();
                            this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                            this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                        }), null);
                        step = 0;
                    }
                }
            }
            // Lógica para iniciar la captura de un punto de calibración
            else if (e.Button.ToLower().Equals("a") || e.Button.ToLower().Equals("b"))
            {
                if (!this.keyMapper.cursorPos.OutOfReach)
                {
                    this.buttonTimer.Start();
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.wiimoteNo.Text = null;
                        this.insText2.Text = "Mantenga presionado";

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
                        this.insText2.Text = "No se encuentran sensores. Asegúrese de estar a la distancia adecuada y apuntando a la pantalla.";

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


            IRState irState = keyMapper.CurrentWiimoteState.IRState;

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.wiimoteNo.Text = null;
                this.insText2.Text = "Suelte";

                this.TextBorder.UpdateLayout();
                this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
            }), null);

            // --- CAPTURA DE COORDENADAS IR SEGÚN EL PASO Y EL MODO CON IF ANIDADO ---
            switch (step)
            {
                case 0: // Captura del Centro (Square, Diamond)
                    if (Settings.Default.pointer_4IRMode == "square")
                    {
                        this.keyMapper.settings.CenterX = (float)((this.keyMapper.cursorPos.RelativeX - 2) * Math.Cos(this.keyMapper.cursorPos.Rotation) - (this.keyMapper.cursorPos.RelativeY - 2) * Math.Sin(this.keyMapper.cursorPos.Rotation) + 2);
                        this.keyMapper.settings.CenterY = (float)((this.keyMapper.cursorPos.RelativeX - 2) * Math.Sin(-this.keyMapper.cursorPos.Rotation) + (this.keyMapper.cursorPos.RelativeY - 2) * Math.Cos(-this.keyMapper.cursorPos.Rotation) + 2);
                        this.keyMapper.settings.TLled = (float)(0.5 - ((this.keyMapper.cursorPos.Width / this.keyMapper.cursorPos.Height) / 4));
                        this.keyMapper.settings.TRled = (float)(0.5 + ((this.keyMapper.cursorPos.Width / this.keyMapper.cursorPos.Height) / 4));
                    }
					else if (Settings.Default.pointer_4IRMode == "diamond")
                    {
                        //this.keyMapper.settings.CenterX = 0.5f;
                        //this.keyMapper.settings.CenterY = 0.5f;
                        float sumX = 0f;
                        float sumY = 0f;
                        int count = 0;

                        for (int i = 0; i < 4; i++)
                        {
                            if (irState.IRSensors[i].Found)
                            {
                                sumX += irState.IRSensors[i].Position.X;
                                sumY += irState.IRSensors[i].Position.Y;
                                count++;
                            }
                        }

                        if (count > 0)
                        {
                            this.keyMapper.settings.CenterX = sumX / count;
                            this.keyMapper.settings.CenterY = sumY / count;
                        }

                    }
                break;
                case 1: // Captura de Inferior Derecha (None, Square) o Superior (Diamond)
                    if (Settings.Default.pointer_4IRMode == "none")
                    {
                        this.keyMapper.settings.Bottom = (float)this.keyMapper.cursorPos.RelativeY;
                        this.keyMapper.settings.Right = (float)this.keyMapper.cursorPos.RelativeX;
                    }
                    else if (Settings.Default.pointer_4IRMode == "square")
                    {
                        bottomOffset = (float)this.keyMapper.cursorPos.LightbarY;
                        rightOffset = (float)this.keyMapper.cursorPos.LightbarX;
                    }
                    else if (Settings.Default.pointer_4IRMode == "diamond")
                    {

                                               
                        int topIndex = -1;
                        float topY = float.MaxValue;

                        for (int i = 0; i < 4; i++)
                        {
                            if (irState.IRSensors[i].Found && irState.IRSensors[i].Position.Y < topY)
                            {
                                topY = irState.IRSensors[i].Position.Y;
                                topIndex = i;
                            }
                        }
                        this.keyMapper.settings.DiamondTopY = 1.0f + irState.IRSensors[topIndex].Position.Y;
                        /*if (topIndex >= 0)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                insText2.Text = $"LED ARRIBA detectado: LED{topIndex} Y={irState.IRSensors[topIndex].Position.Y:0.000}";
                                this.TextBorder.UpdateLayout();
                                this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                            }), null);
                        }*/
                    }
                    break;
                case 2: // Captura de Superior Izquierda (None, Square) o Inferior (Diamond)
                    if (Settings.Default.pointer_4IRMode == "none")
                    {
                        this.keyMapper.settings.Top = (float)this.keyMapper.cursorPos.RelativeY;
                        this.keyMapper.settings.Left = (float)this.keyMapper.cursorPos.RelativeX;
                    }
                    else if (Settings.Default.pointer_4IRMode == "square")
                    {
                        topOffset = (float)this.keyMapper.cursorPos.LightbarY;
                        leftOffset = (float)this.keyMapper.cursorPos.LightbarX;
                    }
                    else if (Settings.Default.pointer_4IRMode == "diamond")
                    {
                        int bottomIndex = -1;
                        float bottomY = float.MinValue;

                        for (int i = 0; i < 4; i++)
                        {
                            if (irState.IRSensors[i].Found && irState.IRSensors[i].Position.Y > bottomY)
                            {
                                bottomY = irState.IRSensors[i].Position.Y;
                                bottomIndex = i;
                            }
                        }
                        this.keyMapper.settings.DiamondBottomY = 0 - (1.0f - irState.IRSensors[bottomIndex].Position.Y);
                        /*if (bottomIndex >= 0)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                insText2.Text = $"LED ABAJO detectado: LED{bottomIndex} Y={irState.IRSensors[bottomIndex].Position.Y:0.000}";
                                this.TextBorder.UpdateLayout();
                                this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                            }), null);
                        }*/
                    }
                    break;
                case 3: // Captura de Izquierda (Diamond)
                    if (Settings.Default.pointer_4IRMode == "diamond")
                    {
                        int leftIndex = -1;
                        float leftX = float.MinValue;

                        for (int i = 0; i < 4; i++)
                        {
                            if (irState.IRSensors[i].Found && irState.IRSensors[i].Position.X > leftX)
                            {
                                leftX = irState.IRSensors[i].Position.X;
                                leftIndex = i;
                            }
                        }
                        this.keyMapper.settings.DiamondLeftX = 0 - (1.0f - irState.IRSensors[leftIndex].Position.X);
                        /*if (leftIndex >= 0)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                insText2.Text = $"LED IZQUIERDO detectado: LED{leftIndex} X={irState.IRSensors[leftIndex].Position.X:0.000}";
                                this.TextBorder.UpdateLayout();
                                this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                            }), null);
                        }*/
                    }
                    break;
                case 4: // Captura de Derecha (Diamond)
                    if (Settings.Default.pointer_4IRMode == "diamond")
                    {
                        {
                            int rightIndex = -1;
                            float rightX = float.MaxValue;

                            for (int i = 0; i < 4; i++)
                            {
                                if (irState.IRSensors[i].Found && irState.IRSensors[i].Position.X < rightX)
                                {
                                    rightX = irState.IRSensors[i].Position.X;
                                    rightIndex = i;
                                }
                            }
                            this.keyMapper.settings.DiamondRightX = 1.0f + irState.IRSensors[rightIndex].Position.X;
                            /*if (rightIndex >= 0)
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    insText2.Text = $"LED DERECHO detectado: LED{rightIndex} X={irState.IRSensors[rightIndex].Position.X:0.000}";
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                            }*/
                        }
                    }
                    break;
                default: break;
            }
        }

        private System.Windows.Point movePoint(double fNormalX, double fNormalY)
        {
            System.Windows.Point tPoint = new System.Windows.Point(fNormalX * this.ActualWidth, fNormalY * this.ActualHeight);

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