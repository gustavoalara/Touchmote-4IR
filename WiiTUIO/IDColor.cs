using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WiiTUIO.Properties;

namespace WiiTUIO
{
    class IDColor
    {
        public static Color getColor(int id)
        {
            switch (id)
            {
                case 1:
                    return Color.FromRgb((byte)Settings.Default.Color_ID1[0], (byte)Settings.Default.Color_ID1[1], (byte)Settings.Default.Color_ID1[2]);
                case 2:
                    return Color.FromRgb((byte)Settings.Default.Color_ID2[0], (byte)Settings.Default.Color_ID2[1], (byte)Settings.Default.Color_ID2[2]);
                case 3:
                    return Color.FromRgb((byte)Settings.Default.Color_ID3[0], (byte)Settings.Default.Color_ID3[1], (byte)Settings.Default.Color_ID3[2]);
                case 4:
                    return Color.FromRgb((byte)Settings.Default.Color_ID4[0], (byte)Settings.Default.Color_ID4[1], (byte)Settings.Default.Color_ID4[2]);
                default:
                    return randomColor();
            }

        }

        public static Color randomColor()
        {
            Random rand = new Random();
            byte red = (byte)rand.Next(255);
            byte green = (byte)rand.Next(255);
            byte blue = (byte)rand.Next(255);

            int kill = rand.Next(2);
            switch (kill)
            {
                case 0:
                    red = 0;
                    break;
                case 1:
                    green = 0;
                    break;
                case 2:
                    blue = 0;
                    break;
            }

            return Color.FromRgb(red,green,blue);
        }
    }
}
