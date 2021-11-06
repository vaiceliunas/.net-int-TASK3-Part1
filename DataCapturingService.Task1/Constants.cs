using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCapturingService.Task1
{
    public static class Constants
    {
        public const string Username = "dataCapturingService";
        public const string Password = "dataCapturingService";
        public const string ExchangeName = "fileTransferExchange";
        public const string RabbitMqAddress = "localhost:5672";
        public const string Path = "C:\\Users\\Arnas_Vaiceliunas\\Documents\\_capturingService";
        public const string Filter = "*.*";
        public const string RoutingKey = "file.transfer";
    }
}
