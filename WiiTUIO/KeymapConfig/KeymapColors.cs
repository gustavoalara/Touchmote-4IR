using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WiiTUIO
{
    public class KeymapColors
    {

        public static Color GetColor(KeymapOutputType type)
        {
            switch (type)
            {
                case KeymapOutputType.KEYBOARD:
                    return Colors.Orange;
                case KeymapOutputType.MOUSE:
                    return Colors.OrangeRed;
                case KeymapOutputType.XINPUT:
                    return Colors.Green;
                case KeymapOutputType.WIIMOTE:
                    return Colors.DodgerBlue;
                case KeymapOutputType.CURSOR:
                    return Colors.Purple;
                case KeymapOutputType.DISABLE:
                    return Colors.Black;
                default:
                    return Colors.Black;
            }
        }

    }
}
