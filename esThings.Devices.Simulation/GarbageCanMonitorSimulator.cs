using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace esThings.Devices.Simulation
{
    public class GarbageCanMonitorSimulator
    {
        private readonly RegistryManager _registryManager;
        private readonly List<GarbageCanMonitor> _monitors = new List<GarbageCanMonitor>();

        public GarbageCanMonitorSimulator()
        {
            string connectionString = ConfigurationManager.AppSettings["IoTHubConnectionString"];
            _registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        }

        public async void StartAsync(int numberOfDevices, int messageIntervalSeconds)
        {
            await CreateMonitorsAsync(numberOfDevices);
            await FillCansAsync(messageIntervalSeconds);
        }

        private async Task CreateMonitorsAsync(int numberOfDevices)
        {
            for (int i = 0; i < numberOfDevices; i++)
            {
                _monitors.Add(await GetMonitorAsync($"garbageCanMonitor{i}"));
            }
        }

        private async Task<GarbageCanMonitor> GetMonitorAsync(string deviceId)
        {
            string hubUri = ConfigurationManager.AppSettings["IoTHubUri"];

            Device device;

            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceId);
            }

            GarbageCanMonitor monitor = new GarbageCanMonitor(device, hubUri);

            Console.WriteLine($"Created monitor {monitor.Id}");

            return monitor;
        }

        private async Task FillCansAsync(int messageIntervalSeconds)
        {
            Random random = new Random();

            while (true)
            {
                foreach (GarbageCanMonitor monitor in _monitors)
                {
                    if (monitor.Fullness >= 100)
                        monitor.Fullness = 0; // "empty" the can
                    else
                    {
                        int fill = random.Next(100);

                        monitor.Fullness += fill;

                        if (monitor.Fullness > 100)
                            monitor.Fullness = 100;
                    }

                    await monitor.SendStatus();
                }

                Task.Delay(messageIntervalSeconds * 1000).Wait();
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
