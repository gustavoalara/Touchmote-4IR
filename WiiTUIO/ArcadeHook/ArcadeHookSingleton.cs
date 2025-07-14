using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiTUIO.ArcadeHook
{
    public static class ArcadeHookSingleton
    {
        private static ArcadeHookMain defaultInstance;

        public static ArcadeHookMain Default
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new ArcadeHookMain();
                }

                return defaultInstance;
            }
        }
    }
}
