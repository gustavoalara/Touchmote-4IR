using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Nefarius.ViGEm.Client;

namespace WiiTUIO.Output.Handlers.Xinput
{
    public class ViGEmBusClient
    {
        private ViGEmClient vigemTestClient = null;
        public ViGEmClient VigemTestClient => vigemTestClient;

        public ViGEmBusClient()
        {
            try
            {
                vigemTestClient = new ViGEmClient();
            }
            catch (Exception) { }
        }

        public void Disconnect()
        {
            if (vigemTestClient != null)
            {
                // Allow some time for controllers to disconnect
                // before disconnecting from ViGEmBus
                Thread.Sleep(500);

                vigemTestClient.Dispose();
                vigemTestClient = null;
            }
        }
    }
}
