using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace ProcessDeviceToCloudMessages
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string iotHubConnectionString = "HostName=esThings.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=fOqVTq6qlMj5ox7w9AJTItDxNzxKTqL8QSjE78xdAlg=";
            const string iotHubD2CEndpoint = "messages/events";
            
            StoreEventProcessor.StorageConnectionString =
                "DefaultEndpointsProtocol=http;AccountName=esthings;AccountKey=EQmh5WvKutnJRcCo/UPEPuhSRjfPTZywqg8XYOF/fQphl8Ngf+T64++LZCCNyQr59srpUzf2Swx0nUbkbkjb1Q==";
            StoreEventProcessor.ServiceBusConnectionString =
                "Endpoint=sb://esthings.servicebus.windows.net/;SharedAccessKeyName=esThingsSend;SharedAccessKey=TM/CQ6LQEdYZ657z6tTUbLCqZUad3ofEOy7wRehLYRU=;EntityPath=esthingsqueue";

            string eventProcessorHostName = Guid.NewGuid().ToString();

            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2CEndpoint,
                EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString,
                StoreEventProcessor.StorageConnectionString, "messages-events");

            Console.WriteLine("Registering EventProcessor...");

            eventProcessorHost.RegisterEventProcessorAsync<StoreEventProcessor>().Wait();
            Console.ReadLine();

            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
