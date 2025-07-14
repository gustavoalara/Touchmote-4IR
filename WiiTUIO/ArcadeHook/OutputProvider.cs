using System;

namespace WiiTUIO.ArcadeHook
{
    public class OutputProvider
    {
        private ArcadeHookMain arcadeHook;
        public Action<string, string> OnOutput;
        int wiiMoteID;

        public OutputProvider(int id)
        {
            this.arcadeHook = ArcadeHookSingleton.Default;
            this.wiiMoteID = id;
            this.arcadeHook.OnExecute += SendOutput;
        }

        public void SendOutput(int id, string key, string value)
        {
            if (id == this.wiiMoteID)
                OnOutput?.Invoke(key, value);
        }
    }
}
