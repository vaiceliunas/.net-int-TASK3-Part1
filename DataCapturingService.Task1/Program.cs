using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DataCapturingService.Task1
{
    internal class Program
    {
        private static void Main()
        {
            RabbitMqActions.InitializeExchange();
            WatcherActions.InitializeWatcher(Constants.Path, Constants.Filter);

            Console.ReadLine();
        }


    }
}
