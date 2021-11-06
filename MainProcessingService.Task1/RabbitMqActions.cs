using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MainProcessingService.Task1
{
    public static class RabbitMqActions
    {
        public static object StatusSendingRateLock = new object();
        public static bool IsListenerInUse = false;
        public static int StatusSendingRate = 1000;
        public static CancellationTokenSource TokenSource = new CancellationTokenSource();
        public static CancellationToken Token = RabbitMqActions.TokenSource.Token;
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
            channel.QueueBind(Constants.QueueName, Constants.ExchangeName, Constants.TransferRoutingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                RabbitMqActions.IsListenerInUse = true;
                var fileName = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["fileName"] as byte[] ?? Array.Empty<byte>());
                var fileId = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["fileId"] as byte[] ?? Array.Empty<byte>());
                var isLastChunk = Convert.ToBoolean(eventArgs.BasicProperties.Headers["isLastChunk"]);


                Helpers.FileSaver(fileName, fileId, isLastChunk, eventArgs.Body.ToArray());
                Console.WriteLine(fileName);
                Console.WriteLine("last chunk? " + isLastChunk);
                Console.WriteLine("fileId " + fileId);
                if(isLastChunk)
                    Console.WriteLine("---------------------------------End of file---------------------------------");
                RabbitMqActions.IsListenerInUse = false;
            };

            channel.BasicConsume(Constants.QueueName, true, consumer);
        }

        public static void SendServiceStatus(string routingKey)
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();

            channel.BasicPublish(Constants.ExchangeName, routingKey);
        }

        public static bool IsListenerBusy()
        {
            return RabbitMqActions.IsListenerInUse;
        }

        public static void SendServiceStatus()
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();

            var status = RabbitMqActions.IsListenerBusy() ? "in use" : "free";
            var statusBytes = Encoding.UTF8.GetBytes(status);


            var headers = new Dictionary<string, object>
            {
                {"time", DateTime.Now.ToString("yyyy-MM-dd hh:ss")}
            };

            var props = channel.CreateBasicProperties();
            props.Headers = headers;
            props.UserId = Constants.Username;

            channel.BasicPublish(Constants.ExchangeName, Constants.GetStatusRoutingKey, true, props, statusBytes);
        }
        public static void InitializeGetInstructionsQueue()
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(Constants.GetInstructionsQueueName, false, false);
            channel.QueueBind(Constants.GetInstructionsQueueName, Constants.ExchangeName, Constants.GetInstructionsRoutingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var ms = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                RabbitMqActions.TokenSource.Cancel();
                RabbitMqActions.TokenSource = new CancellationTokenSource();
                RabbitMqActions.Token = RabbitMqActions.TokenSource.Token;
                RabbitMqActions.SetStatusSendingRate(int.Parse(ms));
                Helpers.InitializeStatusSender(RabbitMqActions.Token);
            };

            channel.BasicConsume(Constants.GetInstructionsQueueName, true, consumer);
        }

        public static void SetStatusSendingRate(int statusSendingRate)
        {
            if (statusSendingRate < 1000)
                return;

            if (statusSendingRate > 5000)
                return;

            lock (StatusSendingRateLock)
            {
                RabbitMqActions.StatusSendingRate = statusSendingRate;
            }
        }
    }
}
