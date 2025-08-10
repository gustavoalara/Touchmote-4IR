using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiTUIO
{
    class Autostart
    {

        public static bool IsAutostart()
        {
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task task = ts.GetTask("Gunmote");
                return task != null;
            }
        }

        public static bool SetAutostart()
        {
            // Get the service on the local machine
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Autostart Gunmote";

                td.Triggers.Add(new LogonTrigger());

                td.Actions.Add(new ExecAction(System.AppDomain.CurrentDomain.BaseDirectory + "Gunmote.exe", null, System.AppDomain.CurrentDomain.BaseDirectory));
                td.Settings.MultipleInstances = TaskInstancesPolicy.StopExisting;
                td.Principal.RunLevel = TaskRunLevel.Highest;

                ts.RootFolder.RegisterTaskDefinition(@"Gunmote", td);

                return true;

                //ts.RootFolder.DeleteTask("Gunmote");

            }
        }

        public static bool UnsetAutostart()
        {
            // Get the service on the local machine
            using (TaskService ts = new TaskService())
            {
                ts.RootFolder.DeleteTask("Gunmote");
                return true;
            }
        }
    }
}
