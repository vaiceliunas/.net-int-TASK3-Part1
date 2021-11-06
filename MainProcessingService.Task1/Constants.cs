using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainProcessingService.Task1
{
    class Constants
    {
        public const string Username = "mainProcessingService";
        public const string Password = "mainProcessingService";
        public const string ExchangeName = "fileTransferExchange";
        public const string QueueName = "fileTransferQueue";
        public const string GetInstructionsQueueName = "ProcessingServiceGetInstructions";
        public const string RabbitMqAddress = "localhost:5672";
        public const string Path = "C:\\Users\\Arnas_Vaiceliunas\\Documents\\_processingService";
        public const string TransferRoutingKey = "file.transfer";
        public const string GetStatusRoutingKey = "service.status";
        public const string GetInstructionsRoutingKey = "central.params";
    }
}
