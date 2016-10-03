using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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

            return View(messages);
        }

        private async Task<string> GetBlobsAsString()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(ConfigurationManager.AppSettings["BlobContainerName"]);

            IEnumerable<CloudBlockBlob> blockBlobs = blobContainer.ListBlobs()
                .Where(blobItem => blobItem.GetType() == typeof(CloudBlockBlob))
                .Cast<CloudBlockBlob>();

            using (MemoryStream stream = new MemoryStream())
            {
                foreach (CloudBlockBlob blockBlob in blockBlobs)
                    await blockBlob.DownloadToStreamAsync(stream);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
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