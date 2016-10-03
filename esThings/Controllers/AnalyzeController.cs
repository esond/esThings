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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            IEnumerable<GarbageCanStatusMessage> messages = DeserializeMessages(blobString);

            ViewBag.BlobContents = blobString;

            //List<GarbageCanStatusMessage> messages = new List<GarbageCanStatusMessage>();

            //messages.Add(new GarbageCanStatusMessage { MessageId = Guid.NewGuid(), DeviceId = "can1", DeviceKey = "theCanNumber1", Fullness = 50 });
            //messages.Add(new GarbageCanStatusMessage { MessageId = Guid.NewGuid(), DeviceId = "can1", DeviceKey = "theCanNumber2", Fullness = 0 });
            //messages.Add(new GarbageCanStatusMessage { MessageId = Guid.NewGuid(), DeviceId = "can2", DeviceKey = "theCanNumber3", Fullness = 100 });

            return View(messages);
        }

        private async Task<string> GetBlobsAsString()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("esthings");

            // TODO: Get this dynamically based on partition (see line 98 in StoreEventProcessor) and/or get all partitions
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("iothubd2c_1"); 

            string contents;

            using (MemoryStream ms = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(ms);
                contents = Encoding.UTF8.GetString(ms.ToArray());
            }

            return contents;
        }

        private IEnumerable<GarbageCanStatusMessage> DeserializeMessages(string data)
        {
            List<GarbageCanStatusMessage> messages = new List<GarbageCanStatusMessage>();

            using (TextReader text = new StringReader(data))
            using (JsonTextReader reader = new JsonTextReader(text) { SupportMultipleContent = true })
            {
                while (reader.Read())
                {
                    //TODO: These messages should get stored and sent as proper, fully-formed JSON objects, not just tokens.
                    JObject jMessage = (JObject)JToken.ReadFrom(reader);

                    GarbageCanStatusMessage message =
                        JsonConvert.DeserializeObject<GarbageCanStatusMessage>(jMessage.ToString());

                    messages.Add(message);
                }
            }

            return messages;
        }
    }
}