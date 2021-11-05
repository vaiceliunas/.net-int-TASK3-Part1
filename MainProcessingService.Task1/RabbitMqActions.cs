using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainProcessingService.Task1
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

        public static void InitializeQueue()
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(Constants.QueueName, false, false);
            channel.QueueBind(Constants.QueueName, Constants.ExchangeName, Constants.RoutingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {

                var fileName = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["fileName"] as byte[] ?? Array.Empty<byte>());
                var fileId = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["fileId"] as byte[] ?? Array.Empty<byte>());
                var isLastChunk = Convert.ToBoolean(eventArgs.BasicProperties.Headers["isLastChunk"]);


                Helpers.FileSaver(fileName, fileId, isLastChunk, eventArgs.Body.ToArray());
                Console.WriteLine(fileName);
                Console.WriteLine("last chunk? " + isLastChunk);
                Console.WriteLine("fileId " + fileId);
                if(isLastChunk)
                    Console.WriteLine("---------------------------------End of file---------------------------------");

            };

            channel.BasicConsume(Constants.QueueName, true, consumer);
        }

    }
}
