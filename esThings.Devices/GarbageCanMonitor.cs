using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace esThings.Devices
{
    public class GarbageCanMonitor
    {
        private readonly DeviceClient _deviceClient;

        private bool _isRunning;

        public GarbageCanMonitor(string hubUri, string id, string deviceKey)
        {
            HubUri = hubUri;
            Id = id;
            DeviceKey = deviceKey;

            _deviceClient = DeviceClient.Create(HubUri, new DeviceAuthenticationWithRegistrySymmetricKey(Id, DeviceKey));
        }

        public string HubUri { get; set; }

        public string Id { get; set; }

        public string DeviceKey { get; set; }

        public int Fullness { get; set; }

        public int MessageIntervalSeconds { get; set; }

        public async void Start()
        {
            _isRunning = true;

            while (_isRunning)
            {
                await SendStatus();

                Task.Delay(MessageIntervalSeconds * 1000).Wait();
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public async Task SendStatus()
        {
            GarbageCanStatus status = new GarbageCanStatus();
            status.MessageId = Guid.NewGuid();
            status.DeviceId = Id;
            status.DeviceKey = DeviceKey;
            status.Fullness = Fullness;

            string messageString = JsonConvert.SerializeObject(status);
            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _deviceClient.SendEventAsync(message);
        }
    }
}

