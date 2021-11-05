using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Framing;

namespace DataCapturingService.Task1
{
    public static class RabbitMqActions
    {
        public static IConnection GetRabbitMqConnection()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri($"amqp://{Constants.Username}:{Constants.Password}@{Constants.RabbitMqAddress}")
            };

            return factory.CreateConnection();
        }

        public static void InitializeExchange()
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();
            //durable - survives rabbitMQ restart
            channel.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Direct, true);

            channel.Close();
            connection.Close();
        }

        public static void SendInChunks(IModel channel, string fileName, string filePath)
        {
            var chunkSize = 4096000;
            var fileStream = File.OpenRead(filePath);
            var totalFileSize = Convert.ToInt32(fileStream.Length);
            var remainingFileSize = totalFileSize;
            var fileId = Guid.NewGuid().ToString();
            var finished = false;
            while (true)
            {
                if (remainingFileSize <= 0) break;
                int read;
                byte[] buffer;
                if (remainingFileSize > chunkSize)
                {
                    buffer = new byte[chunkSize];
                    read = fileStream.Read(buffer, 0, chunkSize);
                }
                else
                {
                    buffer = new byte[remainingFileSize];
                    read = fileStream.Read(buffer, 0, remainingFileSize);
                    finished = true;
                }

                var headers = new Dictionary<string, object>
                {
                    {"fileName", fileName},
                    {"isLastChunk", finished},
                    {"fileId", fileId}
                };

                var props = channel.CreateBasicProperties();
                props.Persistent = true;
                props.Headers = headers;

                channel.BasicPublish(Constants.ExchangeName, Constants.RoutingKey, true, props, buffer);
                remainingFileSize -= read;
            }
        }

    }
}
