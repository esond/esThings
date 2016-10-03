using System;
using Microsoft.Azure.Devices;

namespace esThings.Devices
{
    public class GarbageCanStatusMessage
    {
        public Guid MessageId { get; set; }

        public string DeviceId { get; set; }

        public string DeviceKey { get; set; }

        public int Fullness { get; set; }

        public DateTime MessageSentDateTime { get; set; }

        public bool IsFull => Fullness >= 100;

        public bool IsEmpty => Fullness == 0;
    }
}
