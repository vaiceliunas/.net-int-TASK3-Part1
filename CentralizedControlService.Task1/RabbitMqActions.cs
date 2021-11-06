using RabbitMQ.Client;
using System;
using RabbitMQ.Client.Events;
using System.Text;

namespace CentralizedControlService.Task1
{
    public static class RabbitMqActions
    {
        public static bool WantsToChangeProperties = false;
        public static IConnection GetRabbitMqConnection()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri($"amqp://{Constants.Username}:{Constants.Password}@{Constants.RabbitMqAddress}")
            };

            return factory.CreateConnection();
        }

        public static void InitializeTransferQueue()
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(Constants.QueueName, false, false);
            channel.QueueBind(Constants.QueueName, Constants.ExchangeName, Constants.GetStatusRoutingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var time = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["time"] as byte[] ?? Array.Empty<byte>());
                var user = eventArgs.BasicProperties.UserId;
                var status = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var msg = $"{time}: {user}- {status}";
                Helpers.WriteIntoFile(msg);
                //Console.WriteLine(msg);
            };

            channel.BasicConsume(Constants.QueueName, true, consumer);
        }

        public static void SendInstructions(int ms)
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();
            var msBytes = Encoding.UTF8.GetBytes(ms.ToString());
            channel.BasicPublish(Constants.ExchangeName, Constants.GetInstructionsRoutingKey, true, null, msBytes);
        }

    }
}
