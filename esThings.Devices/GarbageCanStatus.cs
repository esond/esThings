using System;

namespace esThings.Devices
{
    public class GarbageCanMessage
    {
        public Guid MessageId { get; set; }

        public string DeviceId { get; set; }

        public string DeviceKey { get; set; }

        public int Fullness { get; set; }
    }
}
