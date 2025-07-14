using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WiiCPP;
using WiiTUIO.DeviceUtils;
using WiiTUIO.Output;
using WiiTUIO.Properties;

namespace WiiTUIO.Provider
{
    /// <summary>
    /// Interaction logic for WiiPointerProviderSettings.xaml
    /// </summary>
    public partial class WiiPointerProviderSettings : UserControl
    {
        bool initializing = true;

        public WiiPointerProviderSettings()
        {
            InitializeComponent();

            if (Settings.Default.pointer_4IRMode == "none")
            {
                switch (Settings.Default.pointer_sensorBarPos)
                {
                    case "top":
                        this.cbiTop.IsSelected = true;
                        break;
                    case "bottom":
                        this.cbiBottom.IsSelected = true;
                        break;
                    default:
                        this.cbiCenter.IsSelected = true;
                        break;
                }
            }
            else
            {
                switch (Settings.Default.pointer_4IRMode)
                {
                    case "square":
                        this.cbiSquare.IsSelected = true;
                        break;
                    case "diamond":
                        this.cbiDiamond.IsSelected = true;
                        break;
                    default:
                        this.cbiCenter.IsSelected = true;
                        break;
                }
            }

            this.initializing = false;
        }

        private void SBPositionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.initializing)
            {
                if (this.cbiTop.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "none";
                    Settings.Default.pointer_sensorBarPos = "top";
                }
                else if (this.cbiBottom.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "none";
                    Settings.Default.pointer_sensorBarPos = "bottom";
                }
                else if (this.cbiCenter.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "none";
                    Settings.Default.pointer_sensorBarPos = "center";
                }
                else if (this.cbiSquare.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "square";
                }

                else if (this.cbiDiamond.IsSelected)
                {
                    Settings.Default.pointer_4IRMode = "diamond";
                }
            }
        }

        /*
        private void systemCursor_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.pointer_changeSystemCursor = true;
        }

        private void systemCursor_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.pointer_changeSystemCursor = false;
        }
        */
        /*
        private void moveCursor_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.pointer_moveCursor = true;
        }

        private void moveCursor_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.pointer_moveCursor = false;
        }
         * */

    }
}
