# esThings
A research/experimentation project to evaluate the capabilities of the MS Asure IoT hub.

###esThings - ASP.NET MVC Web Application
There are two main pages in this application: "analyze" and "configure".

The "Analyze" page displays a list of all the messages (stored in Block BLOBs) from a configured Azure storage account. These BLOBs are populated using simulated devices (esThings.Devices) , which are controlled by a console application (esThings.Devices.Simulation). These devices send messages to an Azure IoT Hub, and those messages are processed by another console application, ProcessDeviceToCloudMessages.

The "Configure" page is the next stage in the development, and will create cloud-to-device messages intdended to send the simulated devices some sort of congifuation information, which will be processed and distributed through Azure Web Jobs.
