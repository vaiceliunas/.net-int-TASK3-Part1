using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using RabbitMQ.Client.Events;

namespace DataCapturingService.Task1
{
    public static class RabbitMqActions
    {
        public static object StatusSendingRateLock = new object();
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
                props.UserId = Constants.Username;

                channel.BasicPublish(Constants.ExchangeName, Constants.TransferRoutingKey, true, props, buffer);
                remainingFileSize -= read;
            }
        }

        public static void SendServiceStatus()
        {
            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();

            var status = WatcherActions.IsWatcherBusy() ? "in use" : "free";
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
