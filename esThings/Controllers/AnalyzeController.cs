using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using esThings.Devices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace esThings.Controllers
{
    public class AnalyzeController : Controller
    {
        private string _iotHubConnectionString = ConfigurationManager.AppSettings["IoTHubConnectionString"];
        private string _iotHubD2CEndpoint = "messages/events";
        private readonly string _storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
        private string _serviceBusConnectionString = ConfigurationManager.AppSettings["ServiceBusListenConnectionString"];

        // GET: Analyze
        public async Task<ActionResult> Index()
        {
            string blobString = await GetBlobsAsString();

            ViewBag.BlobContents = blobString;

            List<GarbageCanStatus> statuses = new List<GarbageCanStatus>();

            statuses.Add(new GarbageCanStatus {MessageId = Guid.NewGuid(), DeviceId = "can1", DeviceKey = "theCanNumber1", Fullness = 50});
            statuses.Add(new GarbageCanStatus {MessageId = Guid.NewGuid(), DeviceId = "can2", DeviceKey = "theCanNumber2", Fullness = 100});

            return View(statuses);
        }

        private async Task<string> GetBlobsAsString()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("esthings");

            // TODO: Get this dynamically based on partition (see line 98 in StoreEventProcessor)
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("iothubd2c_1"); 

            string contents;

            using (MemoryStream ms = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(ms);
                contents = Encoding.UTF8.GetString(ms.ToArray());
            }

            return contents;
        }
    }
}