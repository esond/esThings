using System;
using System.Configuration;

namespace esThings.Devices.Simulation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting simulated GarbageCanMonitors...");
            GarbageCanMonitorSimulator simulator = new GarbageCanMonitorSimulator();

            int messageIntervalSeconds = int.Parse(ConfigurationManager.AppSettings["MessageIntervalSeconds"]);

            simulator.StartAsync(5, messageIntervalSeconds);

            Console.ReadLine();
        }
    }
}
