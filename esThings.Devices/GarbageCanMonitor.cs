using System;
using System.Threading.Tasks;

namespace esThings.Devices
{
    public class GarbageCanMonitor
    {
        private bool _isRunning;

        public GarbageCanMonitor(string hubUri, string id, string deviceKey)
        {
            HubUri = hubUri;
            Id = id;
            DeviceKey = deviceKey;
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
                await SendUpdate();

                Task.Delay(MessageIntervalSeconds * 1000).Wait();
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public async Task SendUpdate()
        {
            throw new NotImplementedException();
        }
    }
}

