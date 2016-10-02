using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ProcessDeviceToCloudMessages
{
    public class StoreEventProcessor : IEventProcessor
    {
        private const int MaxBlockSize = 4 * 1024 * 1024;
        public static string StorageConnectionString;
        public static string ServiceBusConnectionString;

        private readonly CloudBlobClient blobClient;
        private readonly CloudBlobContainer _blobContainer;
        private readonly QueueClient queueClient;

        private long _currentBlockInitOffset;
        private MemoryStream _toAppend = new MemoryStream(MaxBlockSize);

        private Stopwatch _stopwatch;
        private readonly TimeSpan _maxCheckpointTime = TimeSpan.FromHours(1);

        public StoreEventProcessor()
        {
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference("d2ctutorial");
            _blobContainer.CreateIfNotExists();
            queueClient = QueueClient.CreateFromConnectionString(ServiceBusConnectionString);
        }

        Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            return Task.FromResult<object>(null);
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("StoreEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);

            if (!long.TryParse(context.Lease.Offset, out _currentBlockInitOffset))
            {
                _currentBlockInitOffset = 0;
            }
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                byte[] data = eventData.GetBytes();

                if (eventData.Properties.ContainsKey("messageType") && (string)eventData.Properties["messageType"] == "interactive")
                {
                    var messageId = (string)eventData.SystemProperties["message-id"];

                    var queueMessage = new BrokeredMessage(new MemoryStream(data));
                    queueMessage.MessageId = messageId;
                    queueMessage.Properties["messageType"] = "interactive";
                    await queueClient.SendAsync(queueMessage);

                    WriteHighlightedMessage($"Received interactive message: {messageId}");
                    continue;
                }

                if (_toAppend.Length + data.Length > MaxBlockSize || _stopwatch.Elapsed > _maxCheckpointTime)
                {
                    await AppendAndCheckpoint(context);
                }
                await _toAppend.WriteAsync(data, 0, data.Length);

                Console.WriteLine($"Message received.  Partition: '{context.Lease.PartitionId}', " +
                                  $"Data: '{Encoding.UTF8.GetString(data)}'");
            }
        }

        private async Task AppendAndCheckpoint(PartitionContext context)
        {
            var blockIdString = $"startSeq:{_currentBlockInitOffset:0000000000000000000000000}";
            var blockId = Convert.ToBase64String(Encoding.ASCII.GetBytes(blockIdString));
            _toAppend.Seek(0, SeekOrigin.Begin);
            byte[] md5 = MD5.Create().ComputeHash(_toAppend);
            _toAppend.Seek(0, SeekOrigin.Begin);

            var blobName = $"iothubd2c_{context.Lease.PartitionId}";
            var currentBlob = _blobContainer.GetBlockBlobReference(blobName);

            if (await currentBlob.ExistsAsync())
            {
                await currentBlob.PutBlockAsync(blockId, _toAppend, Convert.ToBase64String(md5));
                var blockList = await currentBlob.DownloadBlockListAsync();
                var newBlockList = new List<string>(blockList.Select(b => b.Name));

                if (newBlockList.Any() && newBlockList.Last() != blockId)
                {
                    newBlockList.Add(blockId);
                    WriteHighlightedMessage($"Appending block id: {blockIdString} to blob: {currentBlob.Name}");
                }
                else
                {
                    WriteHighlightedMessage($"Overwriting block id: {blockIdString}");
                }
                await currentBlob.PutBlockListAsync(newBlockList);
            }
            else
            {
                await currentBlob.PutBlockAsync(blockId, _toAppend, Convert.ToBase64String(md5));
                var newBlockList = new List<string>();
                newBlockList.Add(blockId);
                await currentBlob.PutBlockListAsync(newBlockList);

                WriteHighlightedMessage("Created new blob");
            }

            _toAppend.Dispose();
            _toAppend = new MemoryStream(MaxBlockSize);

            // checkpoint.
            await context.CheckpointAsync();
            WriteHighlightedMessage($"Checkpointed partition: {context.Lease.PartitionId}");

            _currentBlockInitOffset = long.Parse(context.Lease.Offset);
            _stopwatch.Restart();
        }

        private void WriteHighlightedMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
