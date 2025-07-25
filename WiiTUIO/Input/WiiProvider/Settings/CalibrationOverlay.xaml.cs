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
using WiimoteLib;
using System.Windows.Shapes; // Required for Line and Polygon
using System.ComponentModel; // Required for PropertyChangedEventArgs in SettingsChanged
using System.Windows.Media.Media3D;
using System.Globalization;
using WiiTUIO.Filters;
using static WiiTUIO.Resources.Resources; // Assumed to contain localized strings
using System.Windows.Documents;
using System.Linq; // Added for .Any() and .FirstOrDefault()

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

        private int step = 0; // Current calibration step

        private float topOffset;
        private float bottomOffset;
        private float leftOffset;
        private float rightOffset;

        // Properties to backup current values before calibration
        private float topBackup;
        private float bottomBackup;
        private float leftBackup;
        private float rightBackup;
        private float tlBackup;
        private float trBackup;
        private float centerXBackup;
        private float centerYBackup;

        // --- BACKUPS FOR DIAMOND MODE ---
        private float diamondTopYBackup;
        private float diamondBottomYBackup;
        private float diamondLeftXBackup;
        private float diamondRightXBackup;
        // --- END BACKUPS FOR DIAMOND MODE ---

        private double marginXBackup;
        private double marginYBackup;

        private int yMin, yMax, xMin, xMax;
        private PointF[] finalPos = new PointF[4];
        private uint[] see = new uint[4];

        Wiimote Wiimote;

        // Constant for the side length of the triangle
        private const double TRIANGLE_SIDE_LENGTH = 20.0;


        /// <summary>
        /// An event that fires once calibration has finished.
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

            // Compensate for DPI settings
            Loaded += (o, e) =>
            {
                this.updateWindowToScreen(primaryScreen);

                // Prevent OverlayWindow from appearing in the alt+tab menu.
                UIHelpers.HideFromAltTab(this);
                // ADDED CALL: Update lines and triangles when window loads
                UpdateCalibrationLinesAndTrianglesVisibility();
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
            // ADDED CALL: Update lines and triangles after canvas size is updated
            UpdateCalibrationLinesAndTrianglesVisibility();
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
            // ADDED CALL: If 4IR mode changes, we also need to update line and triangle visibility
            else if (e.PropertyName == "pointer_4IRMode")
            {
                UpdateCalibrationLinesAndTrianglesVisibility();
            }
        }

        // NEWLY ADDED METHOD: To update the visibility and position of calibration lines and triangles
        private void UpdateCalibrationLinesAndTrianglesVisibility(SolidColorBrush brush = null)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                // Hide all lines and triangles initially
                VerticalLineLeft.Visibility = Visibility.Hidden;
                VerticalLineRight.Visibility = Visibility.Hidden;
                HorizontalLineCenter.Visibility = Visibility.Hidden;
                VerticalLineCenter.Visibility = Visibility.Hidden;

                TriangleLeftTop.Visibility = Visibility.Hidden;
                TriangleLeftBottom.Visibility = Visibility.Hidden;
                TriangleRightTop.Visibility = Visibility.Hidden;
                TriangleRightBottom.Visibility = Visibility.Hidden;
                TriangleCenterTop.Visibility = Visibility.Hidden;
                TriangleCenterBottom.Visibility = Visibility.Hidden;
                TriangleCenterLeft.Visibility = Visibility.Hidden;
                TriangleCenterRight.Visibility = Visibility.Hidden;

                // Hide all grid lines by default
                GridLineV1.Visibility = Visibility.Hidden;
                GridLineV2.Visibility = Visibility.Hidden;
                GridLineV3.Visibility = Visibility.Hidden;
                GridLineV4.Visibility = Visibility.Hidden;
                GridLineV5.Visibility = Visibility.Hidden;
                GridLineH1.Visibility = Visibility.Hidden;
                GridLineH2.Visibility = Visibility.Hidden;
                GridLineH3.Visibility = Visibility.Hidden;
                GridLineH4.Visibility = Visibility.Hidden;
                GridLineH5.Visibility = Visibility.Hidden;


                SolidColorBrush currentBrush = new SolidColorBrush(Colors.Green); // Default green color

                // If keyMapper is available, use its color for consistency
                if (keyMapper != null)
                {
                    Color pointColor = IDColor.getColor(keyMapper.WiimoteID);
                    pointColor.R = (byte)(pointColor.R * 0.8);
                    pointColor.G = (byte)(pointColor.G * 0.8);
                    pointColor.B = (byte)(pointColor.B * 0.8);
                    currentBrush = new SolidColorBrush(pointColor);
                }

                // Calculate a lighter green color for the grid (even lighter)
                Color lighterGreen = Color.FromArgb(
                    128,
                    (byte)Math.Min(255, currentBrush.Color.R + 100), // Increased brightness
                    (byte)Math.Min(255, currentBrush.Color.G + 100), // Increased brightness
                    (byte)Math.Min(255, currentBrush.Color.B + 100)  // Increased brightness
                );
                SolidColorBrush lighterBrush = new SolidColorBrush(lighterGreen);


                // Calculate the height of an equilateral triangle (distance from vertex to base)
                double triangleHeight = TRIANGLE_SIDE_LENGTH * Math.Sqrt(3) / 2;
                // Half the base of the triangle
                double halfBase = TRIANGLE_SIDE_LENGTH / 2;

                double centerX = this.ActualWidth / 2; // Defined here for use in both modes
                double centerY = this.ActualHeight / 2; // Defined here for use in both modes


                if (Settings.Default.pointer_4IRMode == "none" || Settings.Default.pointer_4IRMode == "square")
                {
                    // Logic for "none" or "square" mode (existing vertical lines)
                    double squareSide = this.ActualHeight; // Assuming the "square" is based on height
                    // Vertical lines extend across the entire height
                    double leftLineX = centerX - (squareSide / 2);
                    double rightLineX = centerX + (squareSide / 2);

                    VerticalLineLeft.X1 = leftLineX;
                    VerticalLineLeft.Y1 = 0;
                    VerticalLineLeft.X2 = leftLineX;
                    VerticalLineLeft.Y2 = this.ActualHeight;
                    VerticalLineLeft.Stroke = currentBrush;
                    VerticalLineLeft.Visibility = Visibility.Visible;

                    VerticalLineRight.X1 = rightLineX;
                    VerticalLineRight.Y1 = 0;
                    VerticalLineRight.X2 = rightLineX;
                    VerticalLineRight.Y2 = this.ActualHeight;
                    VerticalLineRight.Stroke = currentBrush;
                    VerticalLineRight.Visibility = Visibility.Visible;

                    // Triangles for vertical lines (none/square)
                    // Top Left Triangle (base at Y=0, vertex at leftLineX, points downwards)
                    TriangleLeftTop.Points = new PointCollection
                    {
                        new System.Windows.Point(leftLineX, triangleHeight),
                        new System.Windows.Point(leftLineX - halfBase, 0),
                        new System.Windows.Point(leftLineX + halfBase, 0)
                    };
                    TriangleLeftTop.Fill = currentBrush;
                    TriangleLeftTop.Visibility = Visibility.Visible;

                    // Bottom Left Triangle (base at Y=ActualHeight, vertex at leftLineX, points upwards)
                    TriangleLeftBottom.Points = new PointCollection
                    {
                        new System.Windows.Point(leftLineX, this.ActualHeight - triangleHeight),
                        new System.Windows.Point(leftLineX - halfBase, this.ActualHeight),
                        new System.Windows.Point(leftLineX + halfBase, this.ActualHeight)
                    };
                    TriangleLeftBottom.Fill = currentBrush;
                    TriangleLeftBottom.Visibility = Visibility.Visible;

                    // Top Right Triangle (base at Y=0, vertex at rightLineX, points downwards)
                    TriangleRightTop.Points = new PointCollection
                    {
                        new System.Windows.Point(rightLineX, triangleHeight),
                        new System.Windows.Point(rightLineX + halfBase, 0),
                        new System.Windows.Point(rightLineX - halfBase, 0)
                    };
                    TriangleRightTop.Fill = currentBrush;
                    TriangleRightTop.Visibility = Visibility.Visible;

                    // Bottom Right Triangle (base at Y=ActualHeight, vertex at rightLineX, points upwards)
                    TriangleRightBottom.Points = new PointCollection
                    {
                        new System.Windows.Point(rightLineX, this.ActualHeight - triangleHeight),
                        new System.Windows.Point(rightLineX + halfBase, this.ActualHeight),
                        new System.Windows.Point(rightLineX - halfBase, this.ActualHeight)
                    };
                    TriangleRightBottom.Fill = currentBrush;
                    TriangleRightBottom.Visibility = Visibility.Visible;

                    // --- Logic for the grid ---
                    // The grid will have 5 vertical and 5 horizontal lines, creating 4x4 sections.
                    // The first vertical and horizontal lines will be centered.
                    double gridSpacingX = this.ActualWidth / 6; // For 5 vertical lines (6 sections)
                    double gridSpacingY = this.ActualHeight / 6; // For 5 horizontal lines (6 sections)

                    // Define the dash array for dashed lines
                    DoubleCollection dashArray = new DoubleCollection { 2, 2 }; // 2 units on, 2 units off

                    // Vertical grid lines
                    // The central line (GridLineV3) is already at centerX
                    GridLineV1.X1 = centerX - 2 * gridSpacingX; GridLineV1.Y1 = 0; GridLineV1.X2 = centerX - 2 * gridSpacingX; GridLineV1.Y2 = this.ActualHeight; GridLineV1.Stroke = lighterBrush; GridLineV1.StrokeDashArray = dashArray; GridLineV1.Visibility = Visibility.Visible;
                    GridLineV2.X1 = centerX - gridSpacingX; GridLineV2.Y1 = 0; GridLineV2.X2 = centerX - gridSpacingX; GridLineV2.Y2 = this.ActualHeight; GridLineV2.Stroke = lighterBrush; GridLineV2.StrokeDashArray = dashArray; GridLineV2.Visibility = Visibility.Visible;
                    GridLineV3.X1 = centerX; GridLineV3.Y1 = 0; GridLineV3.X2 = centerX; GridLineV3.Y2 = this.ActualHeight; GridLineV3.Stroke = lighterBrush; GridLineV3.StrokeDashArray = dashArray; GridLineV3.Visibility = Visibility.Visible; // Central vertical line
                    GridLineV4.X1 = centerX + gridSpacingX; GridLineV4.Y1 = 0; GridLineV4.X2 = centerX + gridSpacingX; GridLineV4.Y2 = this.ActualHeight; GridLineV4.Stroke = lighterBrush; GridLineV4.StrokeDashArray = dashArray; GridLineV4.Visibility = Visibility.Visible;
                    GridLineV5.X1 = centerX + 2 * gridSpacingX; GridLineV5.Y1 = 0; GridLineV5.X2 = centerX + 2 * gridSpacingX; GridLineV5.Y2 = this.ActualHeight; GridLineV5.Stroke = lighterBrush; GridLineV5.StrokeDashArray = dashArray; GridLineV5.Visibility = Visibility.Visible;

                    // Horizontal grid lines
                    // The central line (GridLineH3) is already at centerY
                    GridLineH1.X1 = 0; GridLineH1.Y1 = centerY - 2 * gridSpacingY; GridLineH1.X2 = this.ActualWidth; GridLineH1.Y2 = centerY - 2 * gridSpacingY; GridLineH1.Stroke = lighterBrush; GridLineH1.StrokeDashArray = dashArray; GridLineH1.Visibility = Visibility.Visible;
                    GridLineH2.X1 = 0; GridLineH2.Y1 = centerY - gridSpacingY; GridLineH2.X2 = this.ActualWidth; GridLineH2.Y2 = centerY - gridSpacingY; GridLineH2.Stroke = lighterBrush; GridLineH2.StrokeDashArray = dashArray; GridLineH2.Visibility = Visibility.Visible;
                    GridLineH3.X1 = 0; GridLineH3.Y1 = centerY; GridLineH3.X2 = this.ActualWidth; GridLineH3.Y2 = centerY; GridLineH3.Stroke = lighterBrush; GridLineH3.StrokeDashArray = dashArray; GridLineH3.Visibility = Visibility.Visible; // Central horizontal line
                    GridLineH4.X1 = 0; GridLineH4.Y1 = centerY + gridSpacingY; GridLineH4.X2 = this.ActualWidth; GridLineH4.Y2 = centerY + gridSpacingY; GridLineH4.Stroke = lighterBrush; GridLineH4.StrokeDashArray = dashArray; GridLineH4.Visibility = Visibility.Visible;
                    GridLineH5.X1 = 0; GridLineH5.Y1 = centerY + 2 * gridSpacingY; GridLineH5.X2 = this.ActualWidth; GridLineH5.Y2 = centerY + 2 * gridSpacingY; GridLineH5.Stroke = lighterBrush; GridLineH5.StrokeDashArray = dashArray; GridLineH5.Visibility = Visibility.Visible;

                }
                else if (Settings.Default.pointer_4IRMode == "diamond")
                {
                    // Logic for "diamond" mode (cross lines)
                    // double centerX = this.ActualWidth / 2; // Already defined above
                    // double centerY = this.ActualHeight / 2; // Already defined above

                    // Horizontal line from right center to left
                    HorizontalLineCenter.X1 = this.ActualWidth;
                    HorizontalLineCenter.Y1 = centerY;
                    HorizontalLineCenter.X2 = 0;
                    HorizontalLineCenter.Y2 = centerY;
                    HorizontalLineCenter.Stroke = currentBrush;
                    HorizontalLineCenter.Visibility = Visibility.Visible;

                    // Vertical line from top center to bottom
                    VerticalLineCenter.X1 = centerX;
                    VerticalLineCenter.Y1 = 0;
                    VerticalLineCenter.X2 = centerX;
                    VerticalLineCenter.Y2 = this.ActualHeight;
                    VerticalLineCenter.Stroke = currentBrush;
                    VerticalLineCenter.Visibility = Visibility.Visible;

                    // Triangles for central lines (diamond)
                    // Top Center Triangle (base at X=centerX, Y=0, points downwards)
                    TriangleCenterTop.Points = new PointCollection
                    {
                        new System.Windows.Point(centerX, triangleHeight),
                        new System.Windows.Point(centerX - halfBase, 0),
                        new System.Windows.Point(centerX + halfBase, 0)
                    };
                    TriangleCenterTop.Fill = currentBrush;
                    TriangleCenterTop.Visibility = Visibility.Visible;

                    // Bottom Center Triangle (base at X=centerX, Y=ActualHeight, points upwards)
                    TriangleCenterBottom.Points = new PointCollection
                    {
                        new System.Windows.Point(centerX, this.ActualHeight - triangleHeight),
                        new System.Windows.Point(centerX - halfBase, this.ActualHeight),
                        new System.Windows.Point(centerX + halfBase, this.ActualHeight)
                    };
                    TriangleCenterBottom.Fill = currentBrush;
                    TriangleCenterBottom.Visibility = Visibility.Visible;

                    // Left Center Triangle (base at Y=centerY, X=0, points right)
                    TriangleCenterLeft.Points = new PointCollection
                    {
                        new System.Windows.Point(triangleHeight, centerY),
                        new System.Windows.Point(0, centerY - halfBase),
                        new System.Windows.Point(0, centerY + halfBase)
                    };
                    TriangleCenterLeft.Fill = currentBrush;
                    TriangleCenterLeft.Visibility = Visibility.Visible;

                    // Right Center Triangle (base at Y=centerY, X=ActualWidth, points left)
                    TriangleCenterRight.Points = new PointCollection
                    {
                        new System.Windows.Point(this.ActualWidth - triangleHeight, centerY),
                        new System.Windows.Point(this.ActualWidth, centerY - halfBase),
                        new System.Windows.Point(this.ActualWidth, centerY + halfBase)
                    };
                    TriangleCenterRight.Fill = currentBrush;
                    TriangleCenterRight.Visibility = Visibility.Visible;
                }
            }), null);
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

                    // Set initial stroke for lines
                    VerticalLineLeft.Stroke = brush;
                    VerticalLineRight.Stroke = brush;
                    HorizontalLineCenter.Stroke = brush; // For diamond mode
                    VerticalLineCenter.Stroke = brush;   // For diamond mode

                    DoubleAnimation animation = UIHelpers.createDoubleAnimation(1.0, 200, false);
                    animation.FillBehavior = FillBehavior.HoldEnd;
                    animation.Completed += delegate (object sender, EventArgs pEvent)
                    {
                        // Animation completed, ready for the first step
                    };
                    this.CalibrationCanvas.BeginAnimation(FrameworkElement.OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);

                    // Call here to ensure lines and triangles update with correct color and visibility after keyMapper is set
                    UpdateCalibrationLinesAndTrianglesVisibility(brush);
                }), null);

                // --- BACKUP CURRENT VALUES AND PREPARE FOR EACH MODE ---
                // Always backup basic top, bottom, left, right settings
                topBackup = this.keyMapper.settings.Top;
                bottomBackup = this.keyMapper.settings.Bottom;
                leftBackup = this.keyMapper.settings.Left;
                rightBackup = this.keyMapper.settings.Right;

                // Capture backup of margins here, as they are used in "none" and restored in "square" if canceled.
                marginXBackup = Settings.Default.CalibrationMarginX;
                marginYBackup = Settings.Default.CalibrationMarginY;

                // BACKUP for SQUARE mode
                centerXBackup = this.keyMapper.settings.CenterX;
                centerYBackup = this.keyMapper.settings.CenterY;
                tlBackup = this.keyMapper.settings.TLled;
                trBackup = this.keyMapper.settings.TRled; // Corrected

                // BACKUP for DIAMOND mode
                diamondTopYBackup = this.keyMapper.settings.DiamondTopY;
                diamondBottomYBackup = this.keyMapper.settings.DiamondBottomY;
                diamondLeftXBackup = this.keyMapper.settings.DiamondLeftX;
                diamondRightXBackup = this.keyMapper.settings.DiamondRightX;

                // Initialize the first step based on the mode
                if (Settings.Default.pointer_4IRMode == "none")
                {
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.movePoint(1 - marginXBackup, 1 - marginYBackup); // Bottom Right Corner
                        this.insText2.Text = AimButtomRight;
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
                        this.movePoint(0.5, 0.5); // Center
                        this.insText2.Text = AimCenter;
                        this.TextBorder.UpdateLayout();
                        this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                        this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                    }), null);
                    step = 0; // Step 0 for center
                }
                else if (Settings.Default.pointer_4IRMode == "diamond")
                {
                    Settings.Default.CalibrationMarginX = 0; // Don't use standard margins
                    Settings.Default.CalibrationMarginY = 0;

                    //this.keyMapper.settings.CenterX = 0.5f;
                    //this.keyMapper.settings.CenterY = 0.5f;

                    centerXBackup = this.keyMapper.settings.CenterX;
                    centerYBackup = this.keyMapper.settings.CenterY;

                    this.keyMapper.settings.Top = 0; // Restore default values for calibration
                    this.keyMapper.settings.Bottom = 1;
                    this.keyMapper.settings.Left = 0;
                    this.keyMapper.settings.Right = 1;

                    this.keyMapper.settings.DiamondTopY = 1.0f; // Default value
                    this.keyMapper.settings.DiamondBottomY = 0.0f; // Default value
                    this.keyMapper.settings.DiamondLeftX = 0.0f; // Default value
                    this.keyMapper.settings.DiamondRightX = 1.0f; // Default value

                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.movePoint(0.5, 0.5); // Center
                        this.insText2.Text = AimCenter;
                        this.TextBorder.UpdateLayout();
                        this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                        this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                    }), null);
                    step = 0; // Step 0 for center
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
                        // Ensure lines and triangles are hidden when overlay is hidden
                        UpdateCalibrationLinesAndTrianglesVisibility();
                    };
                    this.CalibrationCanvas.BeginAnimation(FrameworkElement.OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
                }), null);
                step = 0; // Reset step counter when hiding
            }
        }

        private void finishedCalibration()
        {
            // Only save Settings.Default if mode is not "none", as only 4IRMode modifies them
            if (Settings.Default.pointer_4IRMode != "none")
            {
                // If square, save Settings so CenterX, CenterY, TLled, TRled changes persist
                // and restored calibration margins.
                Settings.Default.Save();
            }
            // None and diamond modes don't need Settings.Default.Save() here
            // as their relevant properties are saved in WiimoteSettings.SaveCalibrationData()

            this.keyMapper.settings.SaveCalibrationData(); // Saves Wiimote calibration

            this.HideOverlay();
            // Ensure lines and triangles are hidden after calibration finishes
            UpdateCalibrationLinesAndTrianglesVisibility();
        }

        public void CancelCalibration()
        {
            // Restore backup values based on mode
            this.keyMapper.settings.Top = topBackup;
            this.keyMapper.settings.Bottom = bottomBackup;
            this.keyMapper.settings.Left = leftBackup;
            this.keyMapper.settings.Right = rightBackup;

            if (Settings.Default.pointer_4IRMode == "square")
            {
                this.keyMapper.settings.CenterX = centerXBackup;
                this.keyMapper.settings.CenterY = centerYBackup;
                this.keyMapper.settings.TLled = tlBackup;
                this.keyMapper.settings.TRled = trBackup; // Restore TRled too

                // Make sure to also restore calibration margins for square mode
                // as they are reset to 0 when calibration starts in square.
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


            this.keyMapper.settings.SaveCalibrationData(); // Saves restored values

            this.HideOverlay();
            // Ensure lines and triangles are hidden after calibration is canceled
            UpdateCalibrationLinesAndTrianglesVisibility();
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
                    // Reset general instruction
                    this.insText2.Text = AimTargets;

                    this.TextBorder.UpdateLayout();
                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                }), null);

                if (this.timerElapsed)
                {
                    // --- CALIBRATION STEP ADVANCE LOGIC WITH NESTED IF BY MODE ---
                    switch (step)
                    {
                        case 0:
                            if (Settings.Default.pointer_4IRMode == "square")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(1 - marginXBackup, 1 - marginYBackup); // Bottom Right Corner
                                    this.insText2.Text = AimButtomRight;
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
                                    this.movePoint(0.5, 0); // Top-Center
                                    this.insText2.Text = AimTopCenter;
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
                                    this.insText2.Text = AimTopLeft;
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
                                    this.movePoint(0.5, 1); // Bottom-Center
                                    this.insText2.Text = AimButtomCenter;
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 2;
                            }
                            break;
                        case 2:
                            // This is the last calibration point for "none" and "square"
                            if (Settings.Default.pointer_4IRMode == "none" || Settings.Default.pointer_4IRMode == "square")
                            {
                                // Final assignment logic for square (since Top, Bottom, Left, Right are assigned in buttonTimer_Elapsed)
                                if (Settings.Default.pointer_4IRMode == "square")
                                {
                                    this.keyMapper.settings.Top = topOffset;
                                    this.keyMapper.settings.Bottom = bottomOffset;
                                    this.keyMapper.settings.Left = leftOffset;
                                    this.keyMapper.settings.Right = rightOffset;
                                    // CenterX, CenterY, TLled and TRled values were captured in step 0
                                    // And Top, Bottom, Left, Right in steps 1 and 2
                                    Settings.Default.CalibrationMarginX = marginXBackup; // Restore margins
                                    Settings.Default.CalibrationMarginY = marginYBackup;
                                }

                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.CalibrationPoint.Visibility = Visibility.Hidden; // Hide target
                                    this.wiimoteNo.Text = null;
                                    this.insText2.Text = AimConfirm;
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 5; // Go directly to unified confirmation step
                            }
                            else if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(0, 0.5); // Left-Center
                                    this.insText2.Text = AimLeftCenter;
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 3;
                            }
                            break;
                        case 3:
                            // This step is only reached in "diamond" for right-center
                            if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.movePoint(1, 0.5); // Right-Center
                                    this.insText2.Text = AimRightCenter;
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 4;
                            }
                            break;
                        case 4:
                            // This is the last calibration point for "diamond"
                            if (Settings.Default.pointer_4IRMode == "diamond")
                            {
                                // No assignment logic needed here, as DiamondTopY, BottomY, LeftX, RightX
                                // are assigned directly in buttonTimer_Elapsed in their respective steps.
                                // Also no Settings.Default calibration margins to restore for this mode.

                                Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    this.CalibrationPoint.Visibility = Visibility.Hidden; // Hide target
                                    this.wiimoteNo.Text = null;
                                    this.insText2.Text = AimConfirm;
                                    this.TextBorder.UpdateLayout();
                                    this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                                    this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                                }), null);
                                step = 5; // Go directly to unified confirmation step
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
            // Calibration confirmation or reset logic
            bool isConfirmStep = (step == 5);

            if (isConfirmStep)
            {
                if (e.Button.ToLower().Equals("a"))
                {
                    finishedCalibration();
                }
                else if (e.Button.ToLower().Equals("b"))
                {
                    // Restart calibration based on mode
                    if (Settings.Default.pointer_4IRMode == "none")
                    {
                        Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            this.movePoint(1 - marginXBackup, 1 - marginYBackup); // Return to first point of 'none' mode
                            this.insText2.Text = AimButtomRight;
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
                            this.movePoint(0.5, 0.5); // Return to center
                            this.insText2.Text = AimCenter;
                            this.TextBorder.UpdateLayout();
                            this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                            this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
                        }), null);
                        step = 0;
                    }
                }
            }
            // Logic to start capturing a calibration point
            else if (e.Button.ToLower().Equals("a") || e.Button.ToLower().Equals("b"))
            {
                if (!this.keyMapper.cursorPos.OutOfReach)
                {
                    this.buttonTimer.Start();
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        this.wiimoteNo.Text = null;
                        this.insText2.Text = HoldDown;

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
                        this.insText2.Text = NoSensors;

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
                this.insText2.Text = ReleaseText;

                this.TextBorder.UpdateLayout();
                this.TextBorder.SetValue(Canvas.LeftProperty, 0.5 * this.ActualWidth - (this.TextBorder.ActualWidth / 2));
                this.TextBorder.SetValue(Canvas.TopProperty, 0.25 * this.ActualHeight - (this.TextBorder.ActualHeight / 2));
            }), null);

            // --- CAPTURE IR COORDINATES BASED ON STEP AND MODE WITH NESTED IF ---
            switch (step)
            {
                case 0: // Center Capture (Square, Diamond)
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
                case 1: // Bottom Right Capture (None, Square) or Top (Diamond)
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
                case 2: // Top Left Capture (None, Square) or Bottom (Diamond)
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
                case 3: // Left Capture (Diamond)
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
                case 4: // Right Capture (Diamond)
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
