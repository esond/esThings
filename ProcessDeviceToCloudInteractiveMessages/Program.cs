using System;
using System.IO;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace ProcessDeviceToCloudInteractiveMessages
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Process D2C Interactive Messages\n");

            const string connectionString =
                "Endpoint=sb://esthings.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=2g5JD0rY3MptF34GLOoOpfCPE4u0VicgonKDzY8eLlM=";
            QueueClient client = QueueClient.CreateFromConnectionString(connectionString);

            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            client.OnMessage(message =>
            {
                try
                {
                    Stream bodyStream = message.GetBody<Stream>();
                    bodyStream.Position = 0;

                    string bodyAsString = new StreamReader(bodyStream, Encoding.ASCII).ReadToEnd();

                    Console.WriteLine($"Received message: {bodyAsString} messageId: {message.MessageId}");

                    message.Complete();
                }
                catch (Exception)
                {
                    message.Abandon();
                }
            }, options);

            Console.WriteLine("Receiving interactive messages from SB queue...");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
