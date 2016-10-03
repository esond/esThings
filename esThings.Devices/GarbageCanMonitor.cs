using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace esThings.Devices
{
    public class GarbageCanMonitor
    {
        private DeviceClient _deviceClient;
        private readonly Device _device;

        public GarbageCanMonitor(Device device, string hubUri)
        {
            _device = device;
            HubUri = hubUri;
        }

        public string Id => _device.Id;

        public string HubUri { get; set; }

        public string DeviceKey => _device.Authentication.SymmetricKey.PrimaryKey;

        public int Fullness { get; set; }

        public int MessageIntervalSeconds { get; set; }

        public async Task SendStatus()
        {
            if (_deviceClient == null)
                _deviceClient = DeviceClient.Create(HubUri,
                    new DeviceAuthenticationWithRegistrySymmetricKey(Id, DeviceKey));

            GarbageCanStatusMessage statusMessage = new GarbageCanStatusMessage();
            statusMessage.MessageId = Guid.NewGuid();
            statusMessage.DeviceId = Id;
            statusMessage.DeviceKey = DeviceKey;
            statusMessage.Fullness = Fullness;
            statusMessage.MessageSentDateTime = DateTime.UtcNow;

            // TODO: Serialize these as proper JSON objects, not just tokens.
            string messageString = JsonConvert.SerializeObject(statusMessage);
            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            Console.WriteLine($"{Id} > Sending message: {messageString}");

            await _deviceClient.SendEventAsync(message);
        }
    }
}

