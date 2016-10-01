using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace ReadDeviceToCloudMessages
{
    public class Program
    {
        public static string ConnectionString =
            "HostName=esThings.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=fOqVTq6qlMj5ox7w9AJTItDxNzxKTqL8QSjE78xdAlg=";
        public static string IotHubD2CEndpoint = "messages/events";
        public static EventHubClient EventHubClient;

        public static void Main(string[] args)
        {
            Console.WriteLine("Receive messages. Ctrl-C to exit.\n");

            EventHubClient = EventHubClient.CreateFromConnectionString(ConnectionString, IotHubD2CEndpoint);

            string[] d2CPartitions = EventHubClient.GetRuntimeInformation().PartitionIds;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
                Console.WriteLine("Exiting...");
            };

            List<Task> tasks = new List<Task>();

            foreach (string partition in d2CPartitions)
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cancellationTokenSource.Token));

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// This method uses an EventHubReceiver instance to receive messages from all the IoT hub device-to-cloud receive partitions.
        /// Passing DateTime.Now when creating the EventHubReceiver makes it only recieve messages after it stars. This is useful
        /// in a test environment to seet he current set of messages. In production, one would make sure to process all messages.
        /// </summary>
        /// <param name="partitionId">The partitionId for the Event Hubs partition to receive events from.</param>
        /// <param name="ct"></param>
        /// <seealso cref="http://azure.microsoft.com/en-gb/documentation/articles/iot-hub-csharp-csharp-process-d2c/"/>
        /// <returns></returns>
        private static async Task ReceiveMessagesFromDeviceAsync(string partitionId, CancellationToken ct)
        {
            EventHubReceiver eventHubReceiver = EventHubClient.GetDefaultConsumerGroup().CreateReceiver(partitionId, DateTime.UtcNow);

            while (true)
            {
                if (ct.IsCancellationRequested)
                    break;

                EventData eventData = await eventHubReceiver.ReceiveAsync();

                if (eventData == null)
                    continue;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partitionId, data);
            }
        }
    }
}
