using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace reporting_inventory_aging
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                SyncService ss = new SyncService();
                ss.TestStartupAndStop(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new SyncService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
