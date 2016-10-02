using System;
using System.Threading.Tasks;

namespace esThings.Devices
{
    public class GarbageCanMonitor
    {
        public GarbageCanMonitor(int id, string deviceKey)
        {
            Id = id;
            DeviceKey = deviceKey;
        }

        public int Id { get; set; }

        public string DeviceKey { get; set; }

        public int Fullness { get; set; }

        public int MessageIntervalSeconds { get; set; }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public async Task SendUpdate()
        {
            throw new NotImplementedException();
        }
    }
}

