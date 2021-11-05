using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralizedControlService.Task1
{
    public static class Helpers
    {
        public static void WriteIntoFile(string msg)
        {
            string createText = msg + Environment.NewLine;
            File.AppendAllText(Constants.ServiceStatusesTxtPath, createText);
        }
    }
}
