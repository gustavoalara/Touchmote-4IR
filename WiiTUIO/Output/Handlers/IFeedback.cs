using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiTUIO.Output.Handlers
{
    public interface IFeedback
    {
        Action<Byte, Byte> OnRumble { get; set; }
        Action<int, bool> OnLED { get; set; }
        Action<string> OnSpeaker { get; set; }
    }
}
