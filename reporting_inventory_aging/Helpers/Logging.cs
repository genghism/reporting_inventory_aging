using System;
using System.IO;

namespace reporting_inventory_aging
{
    class Logging
    {
        public static void LogError(string path, string location, string message)
        {
            File.AppendAllText(path, "---------------------------------------------" + "\r\n" + DateTime.Now + "\r\n" + location + "\r\n" + message + "\r\n" + "---------------------------------------------" + "\r\n");
        }
    }
}
