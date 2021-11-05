using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralizedControlService.Task1
{
    public static class Constants
    {
        public const string Username = "centralizedControlService";
        public const string Password = "centralizedControlService";
        public const string ExchangeName = "fileTransferExchange";
        public const string RabbitMqAddress = "localhost:5672";
        public const string QueueName = "ServiceStatusQueue";
        public const string Path = "C:\\Users\\Arnas_Vaiceliunas\\Documents\\_capturingService";
        public const string ServiceStatusesTxtPath = "C:\\Users\\Arnas_Vaiceliunas\\Documents\\_centralizedService\\StatusOfServices.txt";
        public const string Filter = "*.*";
        public const string TransferRoutingKey = "transfer.all";
        public const string GetStatusRoutingKey = "status.get";
        public const string GetInstructionsRoutingKey = "central.instructions";
    }
}
