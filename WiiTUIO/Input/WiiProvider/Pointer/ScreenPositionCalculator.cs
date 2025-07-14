using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using WiimoteLib;
using WiiTUIO.Filters;
using WiiTUIO.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Point = WiimoteLib.Point;

namespace WiiTUIO.Provider
{
    public class ScreenPositionCalculator
    {
        private int wiimoteId = 0;

        private int minXPos;
        private int maxXPos;
        private int maxWidth;

        private uint[] see = new uint[4];

        private PointF median;

        private PointF[] finalPos = new PointF[4];

        private float xDistTop;
        private float xDistBottom;
        private float yDistLeft;
        private float yDistRight;

        float angleTop;
        float angleBottom;
        float angleLeft;
        float angleRight;

        double angle;
        float height;
        float width;

        private float[] angleOffset = new float[4];

        private int minYPos;
        private int maxYPos;
        private int maxHeight;
        private int SBPositionOffset;
        private double CalcMarginOffsetY;

        private double midMarginX;
        private double midMarginY;
        private double marginBoundsX;
        private double marginBoundsY;

        private PointF topLeftPt = new PointF();
        private PointF bottomRightPt = new PointF();
        private PointF trueTopLeftPt = new PointF();
        private PointF trueBottomRightPt = new PointF();
        private double boundsX;
        private double boundsY;

        // Use 0.0 to mean use full mapped range
        private double targetAspectRatio = 0.0;

        private double smoothedX, smoothedZ;
        private int orientation;

        private int leftPoint = -1;

        private Warper pWarper;

        private CursorPos lastPos;

        private Screen primaryScreen;

        private RadiusBuffer smoothingBuffer;
        private CoordFilter coordFilter;

        private int lastIrPoint1 = -1;
        private int lastIrPoint2 = -1;

        public CalibrationSettings settings;

        private float angleTR, angleBR, angleBL, angleTL;
        private float offsetTR, offsetBR, offsetBL, offsetTL;
        private float DistTR, DistBR, DistBL, DistTL;
        private int yMin, yMax, xMin, xMax;

        public ScreenPositionCalculator(int id, CalibrationSettings settings)
        {
            this.wiimoteId = id;
            this.settings = settings;
            this.pWarper = new Warper(this.settings);
            this.primaryScreen = DeviceUtils.DeviceUtil.GetScreen(Settings.Default.primaryMonitor);
            this.recalculateScreenBounds(this.primaryScreen);

            Settings.Default.PropertyChanged += SettingsChanged;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            this.settings.PropertyChanged += SettingsChanged;

            lastPos = new CursorPos(0, 0, 0, 0, 0);

            coordFilter = new CoordFilter();
            this.smoothingBuffer = new RadiusBuffer(Settings.Default.pointer_positionSmoothing);
        }

        private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "primaryMonitor")
            {
                this.primaryScreen = DeviceUtils.DeviceUtil.GetScreen(Settings.Default.primaryMonitor);
                Console.WriteLine("Setting primary monitor for screen position calculator to " + this.primaryScreen.Bounds);
                this.recalculateScreenBounds(this.primaryScreen);
            }
            else if (e.PropertyName == "Left" || e.PropertyName == "Right" || e.PropertyName == "Top" || e.PropertyName == "Bottom")
            {
                trueTopLeftPt.X = topLeftPt.X = this.settings.Left;
                trueTopLeftPt.Y = topLeftPt.Y = this.settings.Top;
                trueBottomRightPt.X = bottomRightPt.X = this.settings.Right;
                trueBottomRightPt.Y = bottomRightPt.Y = this.settings.Bottom;
                recalculateLightgunCoordBounds();
            }
            else if (e.PropertyName == "CalibrationMarginX" || e.PropertyName == "CalibrationMarginY")
            {
                recalculateLightgunCoordBounds();
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            this.primaryScreen = DeviceUtils.DeviceUtil.GetScreen(Settings.Default.primaryMonitor);
            recalculateScreenBounds(this.primaryScreen);
        }

        private void recalculateScreenBounds(Screen screen)
        {
            Console.WriteLine("Setting primary monitor for screen position calculator to " + this.primaryScreen.Bounds);
            minXPos = -(int)(screen.Bounds.Width * Settings.Default.pointer_marginsLeftRight);
            maxXPos = screen.Bounds.Width + (int)(screen.Bounds.Width * Settings.Default.pointer_marginsLeftRight);
            maxWidth = maxXPos - minXPos;
            minYPos = -(int)(screen.Bounds.Height * Settings.Default.pointer_marginsTopBottom);
            maxYPos = screen.Bounds.Height + (int)(screen.Bounds.Height * Settings.Default.pointer_marginsTopBottom);
            maxHeight = maxYPos - minYPos;
            SBPositionOffset = (int)(screen.Bounds.Height * Settings.Default.pointer_sensorBarPosCompensation);
            CalcMarginOffsetY = Settings.Default.pointer_sensorBarPosCompensation;

            midMarginX = Settings.Default.pointer_marginsLeftRight * 0.5;
            midMarginY = Settings.Default.pointer_marginsTopBottom * 0.5;
            marginBoundsX = 1 / (1 - Settings.Default.pointer_marginsLeftRight);
            marginBoundsY = 1 / (1 - Settings.Default.pointer_marginsTopBottom);

            trueTopLeftPt.X = topLeftPt.X = this.settings.Left;
            trueTopLeftPt.Y = topLeftPt.Y = this.settings.Top;
            trueBottomRightPt.X = bottomRightPt.X = this.settings.Right;
            trueBottomRightPt.Y = bottomRightPt.Y = this.settings.Bottom;

            if (targetAspectRatio == 0.0)
            {
                recalculateLightgunCoordBounds();
            }
            else
            {
                RecalculateLightgunAspect(targetAspectRatio);
            }
        }

        private void recalculateLightgunCoordBounds()
        {
            boundsX = (1 - Settings.Default.CalibrationMarginX * 2) / (bottomRightPt.X - topLeftPt.X);
            boundsY = (1 - Settings.Default.CalibrationMarginY * 2) / (bottomRightPt.Y - topLeftPt.Y);
        }

        public CursorPos CalculateCursorPos(WiimoteState wiimoteState)
        {
            int x = 0;
            int y = 0;
            double marginX, marginY = 0.0;
            double lightbarX = 0.0;
            double lightbarY = 0.0;
            int offsetY = 0;
            double marginOffsetY = 0.0;
            PointF resultPos = new PointF();

            IRState irState = wiimoteState.IRState;

            if (Settings.Default.pointer_4IRMode == "none")
            {
                int irPoint1 = 0;
                int irPoint2 = 0;
                bool foundMidpoint = false;
                // First check if previously found points are still detected.
                // Prefer those points first
                if (lastIrPoint1 != -1 && lastIrPoint2 != -1)
                {
                    if (irState.IRSensors[lastIrPoint1].Found &&
                        irState.IRSensors[lastIrPoint2].Found)
                    {
                        foundMidpoint = true;
                        irPoint1 = lastIrPoint1;
                        irPoint2 = lastIrPoint2;
                    }
                }

                // If no midpoint found from previous points, check all available
                // IR points for a possible midpoint
                for (int i = 0; !foundMidpoint && i < irState.IRSensors.Count(); i++)
                {
                    if (irState.IRSensors[i].Found)
                    {
                        for (int j = i + 1; j < irState.IRSensors.Count() && !foundMidpoint; j++)
                        {
                            if (irState.IRSensors[j].Found)
                            {
                                foundMidpoint = true;

                                irPoint1 = i;
                                irPoint2 = j;
                            }
                        }
                    }
                }

                if (foundMidpoint)
                {
                    int i = irPoint1;
                    int j = irPoint2;
                    median.X = (irState.IRSensors[i].Position.X + irState.IRSensors[j].Position.X) / 2.0f;
                    median.Y = (irState.IRSensors[i].Position.Y + irState.IRSensors[j].Position.Y) / 2.0f;

                    smoothedX = smoothedX * 0.9f + wiimoteState.AccelState.RawValues.X * 0.1f;
                    smoothedZ = smoothedZ * 0.9f + wiimoteState.AccelState.RawValues.Z * 0.1f;

                    int l = leftPoint, r;
                    if (leftPoint == -1)
                    {
                        double absx = Math.Abs(smoothedX - 128), absz = Math.Abs(smoothedZ - 128);

                        if (orientation == 0 || orientation == 2) absx -= 5;
                        if (orientation == 1 || orientation == 3) absz -= 5;

                        if (absz >= absx)
                        {
                            if (absz > 5)
                                orientation = (smoothedZ > 128) ? 0 : 2;
                        }
                        else
                        {
                            if (absx > 5)
                                orientation = (smoothedX > 128) ? 3 : 1;
                        }

                        switch (orientation)
                        {
                            case 0: l = (irState.IRSensors[i].RawPosition.X < irState.IRSensors[j].RawPosition.X) ? i : j; break;
                            case 1: l = (irState.IRSensors[i].RawPosition.Y > irState.IRSensors[j].RawPosition.Y) ? i : j; break;
                            case 2: l = (irState.IRSensors[i].RawPosition.X > irState.IRSensors[j].RawPosition.X) ? i : j; break;
                            case 3: l = (irState.IRSensors[i].RawPosition.Y < irState.IRSensors[j].RawPosition.Y) ? i : j; break;
                        }
                    }
                    leftPoint = l;
                    r = l == i ? j : i;

                    double dx = irState.IRSensors[r].RawPosition.X - irState.IRSensors[l].RawPosition.X;
                    double dy = irState.IRSensors[r].RawPosition.Y - irState.IRSensors[l].RawPosition.Y;

                    double d = Math.Sqrt(dx * dx + dy * dy);

                    dx /= d;
                    dy /= d;

                    angle = Math.Atan2(dy, dx);

                    median.X = median.X - 0.5F;
                    median.Y = median.Y - 0.5F;

                    median = this.rotatePoint(median, angle);

                    median.X = median.X + 0.5F;
                    median.Y = median.Y + 0.5F;

                    lastIrPoint1 = irPoint1;
                    lastIrPoint2 = irPoint2;
                }
                else if (!foundMidpoint)
                {
                    CursorPos err = lastPos;
                    err.OutOfReach = true;
                    err.OffScreen = true;
                    leftPoint = -1;
                    lastIrPoint1 = -1;
                    lastIrPoint2 = -1;

                    return err;
                }

                if (Properties.Settings.Default.pointer_sensorBarPos == "top")
                {
                    offsetY = -SBPositionOffset;
                    marginOffsetY = CalcMarginOffsetY;
                }
                else if (Properties.Settings.Default.pointer_sensorBarPos == "bottom")
                {
                    offsetY = SBPositionOffset;
                    marginOffsetY = -CalcMarginOffsetY;
                }

                resultPos = median;
            }
            else if (Settings.Default.pointer_4IRMode == "diamond")
            {
                byte seenFlags = 0;
                double Roll = Math.Atan2(wiimoteState.AccelState.Values.X, wiimoteState.AccelState.Values.Z);

                PointF[] position = new PointF[4];

                for (int i = 0; i < 4; i++)
                {
                    if (irState.IRSensors[i].Found)
                    {
                        position[i] = irState.IRSensors[i].Position;
                        see[i] = (see[i] << 1) | 1;
                        seenFlags |= (byte)(1 << i);
                    }
                    else
                    {
                        see[i] = 0;
                    }
                }

                yMin = yMax = xMin = xMax = -1;
                for (int i = 0; i < 4; i++)
                {
                    if ((seenFlags & (1 << i)) == 0) continue;
                    if (yMin == -1 || position[i].Y < position[yMin].Y) yMin = i;
                    if (yMax == -1 || position[i].Y > position[yMax].Y) yMax = i;
                    if (xMin == -1 || position[i].X < position[xMin].X) xMin = i;
                    if (xMax == -1 || position[i].X > position[xMax].X) xMax = i;
                }

                if (yMin >= 0) finalPos[0] = position[yMin];
                if (xMax >= 0) finalPos[1] = position[xMax];
                if (yMax >= 0) finalPos[2] = position[yMax];
                if (xMin >= 0) finalPos[3] = position[xMin];

                if ((seenFlags & 0x0F) == 0x0F)
                {
                    median.X = (finalPos[0].X + finalPos[1].X + finalPos[2].X + finalPos[3].X) / 4f;
                    median.Y = (finalPos[0].Y + finalPos[1].Y + finalPos[2].Y + finalPos[3].Y) / 4f;

                    float height2 = finalPos[2].Y - finalPos[0].Y;
                    float width2 = finalPos[1].X - finalPos[3].X;
                    height = MathF.Hypot(finalPos[0].Y - finalPos[2].Y, finalPos[0].X - finalPos[2].X);
                    width = MathF.Hypot(finalPos[1].Y - finalPos[3].Y, finalPos[1].X - finalPos[3].X);
                    float angle2 = MathF.Atan2(finalPos[3].Y - finalPos[1].Y, finalPos[1].X - finalPos[3].X);

                    offsetTR = angleTR - angle2;
                    offsetBR = angleBR - angle2;
                    offsetBL = angleBL - angle2;
                    offsetTL = angleTL - angle2;
                }
                else
                {
                    if ((1 << 5 & see[0] & see[1]) != 0)
                    {
                        angleTR = MathF.Atan2(finalPos[0].Y - finalPos[1].Y, finalPos[1].X - finalPos[0].X);
                        DistTR = MathF.Hypot(finalPos[0].Y - finalPos[1].Y, finalPos[0].X - finalPos[1].X);
                        angle = offsetTR - angleTR;
                    }
                    if ((1 << 5 & see[1] & see[2]) != 0)
                    {
                        angleBR = MathF.Atan2(finalPos[1].Y - finalPos[2].Y, finalPos[2].X - finalPos[1].X);
                        DistBR = MathF.Hypot(finalPos[1].Y - finalPos[2].Y, finalPos[1].X - finalPos[2].X);
                        angle = offsetBR - angleBR;
                    }
                    if ((1 << 5 & see[2] & see[3]) != 0)
                    {
                        angleBL = MathF.Atan2(finalPos[2].Y - finalPos[3].Y, finalPos[3].X - finalPos[2].X);
                        DistBL = MathF.Hypot(finalPos[2].Y - finalPos[3].Y, finalPos[2].X - finalPos[3].X);
                        angle = offsetBL - angleBL;
                    }
                    if ((1 << 5 & see[3] & see[0]) != 0)
                    {
                        angleTL = MathF.Atan2(finalPos[3].Y - finalPos[0].Y, finalPos[0].X - finalPos[3].X);
                        DistTL = MathF.Hypot(finalPos[3].Y - finalPos[0].Y, finalPos[3].X - finalPos[0].X);
                        angle = offsetTL - angleTL;
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    if ((seenFlags & (1 << i)) == 0)
                    {
                        float f = (float)angle;
                        float offset = 0;
                        float dist = 0;
                        int refIndex = -1;

                        switch (i)
                        {
                            case 0: offset = offsetTL; dist = DistTL; refIndex = 3; break;
                            case 1: offset = offsetTR; dist = DistTR; refIndex = 0; break;
                            case 2: offset = offsetBR; dist = DistBR; refIndex = 1; break;
                            case 3: offset = offsetBL; dist = DistBL; refIndex = 2; break;
                        }

                        if ((seenFlags & (1 << refIndex)) != 0)
                        {
                            finalPos[i].X = finalPos[refIndex].X + dist * MathF.Cos(offset - f);
                            finalPos[i].Y = finalPos[refIndex].Y + dist * -MathF.Sin(offset - f);
                        }
                    }
                }

                pWarper.setSource(
                        finalPos[1].X, finalPos[1].Y, // Right
                        finalPos[2].X, finalPos[2].Y, // Bottom
                        finalPos[3].X, finalPos[3].Y, // Left
                        finalPos[0].X, finalPos[0].Y  // Top
                    );

                float[] fWarped = pWarper.warp();
                resultPos.X = Math.Min(Math.Max(fWarped[0], 0), 1);
                resultPos.Y = Math.Min(Math.Max(fWarped[1], 0), 1);

                angle = -(MathF.Atan2(finalPos[0].Y - finalPos[1].Y, finalPos[1].X - finalPos[0].X) +
                          MathF.Atan2(finalPos[2].Y - finalPos[3].Y, finalPos[3].X - finalPos[2].X)) / 2;

                if (angle < 0) angle += MathF.PI * 2;

                if (see.Count(seen => seen == 0) >= 3 || double.IsNaN(resultPos.X) || double.IsNaN(resultPos.Y))
                {
                    CursorPos err = lastPos;
                    err.OutOfReach = true;
                    err.OffScreen = true;
                    return err;
                }
            }

            else if (Settings.Default.pointer_4IRMode == "square")
            {
                byte seenFlags = 0;
                double Roll = Math.Atan2(wiimoteState.AccelState.Values.X, wiimoteState.AccelState.Values.Z);
                for (int i = 0; i < 4; i++)
                {
                    if (irState.IRSensors[i].Found)
                    {
                        double point_angle = Math.Atan2(irState.IRSensors[i].Position.Y - median.Y, irState.IRSensors[i].Position.X - median.X) - Roll;
                        if (point_angle < 0) point_angle += 2 * Math.PI;

                        int index = (int)(point_angle / (Math.PI / 2));

                        finalPos[index] = irState.IRSensors[i].Position;
                        see[index] = (see[index] << 1) | 1;
                        seenFlags |= (byte)(1 << index);
                    }
                    else
                        see[i] = 0;
                }

                while ((seenFlags & 15) != 0 && (seenFlags & 15) != 15)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if ((seenFlags & (1 << i)) == 0)
                        {
                            see[i] = 0;
                            int[] neighbors;
                            switch (i)
                            {
                                case 0:
                                    neighbors = new[] { 3, 1 };
                                    break;
                                case 1:
                                    neighbors = new[] { 2, 0 };
                                    break;
                                case 2:
                                    neighbors = new[] { 1, 3 };
                                    break;
                                case 3:
                                    neighbors = new[] { 0, 2 };
                                    break;
                                default:
                                    neighbors = Array.Empty<int>();
                                    break;
                            }

                            foreach (int neighbor in neighbors)
                            {
                                float f = 0;
                                if ((seenFlags & (1 << neighbor)) != 0) // Check if the bit for the neighbor is set
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            f = angleBottom - angleOffset[neighbor];
                                            break;
                                        case 1:
                                            f = angleBottom + (angleOffset[neighbor] - MathF.PI);
                                            break;
                                        case 2:
                                            f = angleTop + angleOffset[neighbor];
                                            break;
                                        case 3:
                                            f = angleTop - (angleOffset[neighbor] - MathF.PI);
                                            break;
                                    }
                                }

                                float distance = 0;
                                switch (i)
                                {
                                    case 0:
                                        distance = (neighbor == 3) ? yDistRight : xDistBottom;
                                        break;
                                    case 1:
                                        distance = (neighbor == 2) ? yDistLeft : xDistBottom;
                                        break;
                                    case 2:
                                        distance = (neighbor == 1) ? yDistLeft : xDistTop;
                                        break;
                                    case 3:
                                        distance = (neighbor == 0) ? yDistRight : xDistTop;
                                        break;
                                }

                                finalPos[i].X = finalPos[neighbor].X + distance * MathF.Cos(f);
                                finalPos[i].Y = finalPos[neighbor].Y + distance * -MathF.Sin(f);
                                seenFlags |= (byte)(1 << i);
                                break;
                            }
                        }
                    }
                    if ((seenFlags & 15) == 15) break;
                }

                pWarper.setSource(finalPos[0].X, finalPos[0].Y, finalPos[1].X, finalPos[1].Y, finalPos[2].X, finalPos[2].Y, finalPos[3].X, finalPos[3].Y);
                float[] fWarped = pWarper.warp();
                resultPos.X = fWarped[0];
                resultPos.Y = fWarped[1];

                if (irState.IRSensors[0].Found == true && irState.IRSensors[1].Found == true && irState.IRSensors[2].Found == true && irState.IRSensors[3].Found == true)
                {
                    median.Y = (irState.IRSensors[0].Position.Y + irState.IRSensors[1].Position.Y + irState.IRSensors[2].Position.Y + irState.IRSensors[3].Position.Y + 0.002f) / 4;
                    median.X = (irState.IRSensors[0].Position.X + irState.IRSensors[1].Position.X + irState.IRSensors[2].Position.X + irState.IRSensors[3].Position.X + 0.002f) / 4;
                }
                else
                {
                    median.Y = (finalPos[0].Y + finalPos[1].Y + finalPos[2].Y + finalPos[3].Y + 0.002f) / 4;
                    median.X = (finalPos[0].X + finalPos[1].X + finalPos[2].X + finalPos[3].X + 0.002f) / 4;
                }

                // If 4 LEDS can be seen and loop has run through 5 times update offsets and height      
                if (((1 << 5) & see[0] & see[1] & see[2] & see[3]) != 0)
                {
                    angleOffset[0] = angleTop - (angleLeft - MathF.PI);
                    angleOffset[1] = -(angleTop - angleRight);
                    angleOffset[2] = -(angleBottom - angleLeft);
                    angleOffset[3] = angleBottom - (angleRight - MathF.PI);
                    height = (yDistLeft + yDistRight) / 2.0f;
                    width = (xDistTop + xDistBottom) / 2.0f;
                }

                // If 2 LEDS can be seen and loop has run through 5 times update angle and distances
                if (((1 << 5) & see[2] & see[1]) != 0)
                {
                    angleLeft = MathF.Atan2(finalPos[1].Y - finalPos[2].Y, finalPos[2].X - finalPos[1].X);
                    yDistLeft = MathF.Hypot((finalPos[2].Y - finalPos[1].Y), (finalPos[2].X - finalPos[1].X));
                }

                if (((1 << 5) & see[0] & see[3]) != 0)
                {
                    angleRight = MathF.Atan2(finalPos[0].Y - finalPos[3].Y, finalPos[3].X - finalPos[0].X);
                    yDistRight = MathF.Hypot((finalPos[0].Y - finalPos[3].Y), (finalPos[0].X - finalPos[3].X));
                }

                if (((1 << 5) & see[2] & see[3]) != 0)
                {
                    angleTop = MathF.Atan2(finalPos[2].Y - finalPos[3].Y, finalPos[3].X - finalPos[2].X);
                    xDistTop = MathF.Hypot((finalPos[2].Y - finalPos[3].Y), (finalPos[2].X - finalPos[3].X));
                }

                if (((1 << 5) & see[0] & see[1]) != 0)
                {
                    angleBottom = MathF.Atan2(finalPos[1].Y - finalPos[0].Y, finalPos[0].X - finalPos[1].X);
                    xDistBottom = MathF.Hypot((finalPos[1].Y - finalPos[0].Y), (finalPos[1].X - finalPos[0].X));
                }

                // Add tilt correction
                angle = -(MathF.Atan2(finalPos[0].Y - finalPos[1].Y, finalPos[1].X - finalPos[0].X) + MathF.Atan2(finalPos[2].Y - finalPos[3].Y, finalPos[3].X - finalPos[2].X)) / 2;
                if (angle < 0) angle += MathF.PI * 2;

                if (see.Count(seen => seen == 0) >= 3 || Double.IsNaN(resultPos.X) || Double.IsNaN(resultPos.Y))
                {
                    CursorPos err = lastPos;
                    err.OutOfReach = true;
                    err.OffScreen = true;

                    return err;
                }
            }

            x = Convert.ToInt32((float)maxWidth * (1 - median.X) + minXPos);
            y = Convert.ToInt32((float)maxHeight * median.Y + minYPos) + offsetY;

            marginX = Math.Min(1.0, Math.Max(0.0, (1 - median.X - midMarginX) * marginBoundsX));
            marginY = Math.Min(1.0, Math.Max(0.0, (median.Y - (marginOffsetY + midMarginX)) * marginBoundsY));

            lightbarX = (resultPos.X - topLeftPt.X) * boundsX + Settings.Default.CalibrationMarginX;
            lightbarY = (resultPos.Y - topLeftPt.Y) * boundsY + Settings.Default.CalibrationMarginY;

            if (x <= 0)
            {
                x = 0;
            }
            else if (x >= primaryScreen.Bounds.Width)
            {
                x = primaryScreen.Bounds.Width - 1;
            }
            if (y <= 0)
            {
                y = 0;
            }
            else if (y >= primaryScreen.Bounds.Height)
            {
                y = primaryScreen.Bounds.Height - 1;
            }

            CursorPos result = new CursorPos(x, y, median.X, median.Y, angle,
                marginX, marginY, lightbarX, lightbarY, width, height);

            if (lightbarX < 0.0 || lightbarX > 1.0 || lightbarY < 0.0 || lightbarY > 1.0)
            {
                result.OffScreen = true;
                result.LightbarX = Math.Min(1.0,
                Math.Max(0.0, lightbarX));
                result.LightbarY = Math.Min(1.0,
                Math.Max(0.0, lightbarY));
            }

            lastPos = result;
            return result;
        }

        private PointF rotatePoint(PointF point, double angle)
        {
            double sin = Math.Sin(angle * -1);
            double cos = Math.Cos(angle * -1);

            double xnew = point.X * cos - point.Y * sin;
            double ynew = point.X * sin + point.Y * cos;

            PointF result;

            xnew = Math.Min(0.5, Math.Max(-0.5, xnew));
            ynew = Math.Min(0.5, Math.Max(-0.5, ynew));

            result.X = (float)xnew;
            result.Y = (float)ynew;

            return result;
        }

        public void RecalculateFullLightgun()
        {
            targetAspectRatio = 0.0;

            topLeftPt = trueTopLeftPt;
            bottomRightPt = trueBottomRightPt;

            recalculateLightgunCoordBounds();
        }

        public void RecalculateLightgunAspect(double targetAspect)
        {
            this.targetAspectRatio = targetAspect;

            int outputWidth = (int)(targetAspect * primaryScreen.Bounds.Height);
            double scaleFactor = outputWidth / (double)primaryScreen.Bounds.Width;
            double target_topLeftX = ((trueBottomRightPt.X + trueTopLeftPt.X) / 2) - ((trueBottomRightPt.X - trueTopLeftPt.X) * scaleFactor / 2);
            double target_bottomRightY = trueBottomRightPt.X - (target_topLeftX - trueTopLeftPt.X);

            topLeftPt = new PointF()
            {
                X = (float)target_topLeftX,
                Y = trueTopLeftPt.Y
            };

            bottomRightPt = new PointF()
            {
                X = (float)target_bottomRightY,
                Y = trueBottomRightPt.Y
            };

            recalculateLightgunCoordBounds();
        }
    }

    public static class MathF
    {
        public const float PI = (float)Math.PI;
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Cos(float d) => (float)Math.Cos(d);
        public static float Round(float a) => (float)Math.Round(a);
        public static float Sin(float a) => (float)Math.Sin(a);
        public static float Hypot(float p, float b) => (float)Math.Sqrt(Math.Pow(p, 2) + Math.Pow(b, 2));
        public static float Sqrt(float d) => (float)Math.Sqrt(d);
        public static float Max(float val1, float val2) => (float)Math.Max(val1, val2);
        public static float Min(float val1, float val2) => (float)Math.Min(val1, val2);
    }
}
