using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace WiiTUIO.Output.Handlers.Xinput
{
    public class ViGEmBus360Device
    {
        public const int outputResolution = 32767 - (-32768);

        private IXbox360Controller cont;
        public event Action<byte, byte> OnRumble;

        public IXbox360Controller Cont { get => cont; }

        public ViGEmBus360Device(ViGEmClient client)
        {
            cont = client.CreateXbox360Controller();
            cont.AutoSubmitReport = false;
            cont.FeedbackReceived += FeedbackProcess;
        }

        private void FeedbackProcess(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            OnRumble?.Invoke(e.LargeMotor, e.SmallMotor);
        }

        public bool Connect()
        {
            cont.Connect();
            return true;
        }

        public bool Disconnect()
        {
            cont.FeedbackReceived -= FeedbackProcess;
            cont.Disconnect();
            cont = null;
            //cont.Dispose();
            return true;
        }

        public bool Update()
        {
            cont.SubmitReport();
            return true;
        }

        public void Reset()
        {
            cont.ResetReport();
            //report = new Xbox360Report();
        }
    }
}
