using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace CreateDeviceIdentity
{
    public class Program
    {
        public static RegistryManager RegistryManager;
        public static string ConnectionString =
            "HostName=esThings.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=fOqVTq6qlMj5ox7w9AJTItDxNzxKTqL8QSjE78xdAlg=";

        public static void Main(string[] args)
        {
            RegistryManager = RegistryManager.CreateFromConnectionString(ConnectionString);

            AddDeviceAsync().Wait();
            
            Console.ReadLine();
        }

        private static async Task AddDeviceAsync()
        {
            string deviceId = "esDevice";

            Device device;

            try
            {
                device = await RegistryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await RegistryManager.GetDeviceAsync(deviceId);
            }

            Console.WriteLine($"Generated device key: {device.Authentication.SymmetricKey.PrimaryKey}");
        }
    }
}
