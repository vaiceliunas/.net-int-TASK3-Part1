using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RabbitMQ.Client.Events;

namespace MainProcessingService.Task1
{
    internal class Program
    {
        private static void Main()
        {
            RabbitMqActions.InitializeQueue();
            Console.ReadLine();
        }
    }
}
