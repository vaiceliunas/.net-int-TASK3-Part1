using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace MainProcessingService.Task1
{
    internal class Program
    {
        private static void Main()
        {
            RabbitMqActions.InitializeTransferQueue();
            RabbitMqActions.InitializeGetInstructionsQueue();
            Helpers.InitializeStatusSender(RabbitMqActions.Token);

            Console.ReadLine();
        }
    }
}
