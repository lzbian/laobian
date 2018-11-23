using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laobian.Common.Azure;
using Laobian.Common.Base;
using Laobian.Common.Notification;
using Microsoft.Extensions.Hosting;

namespace Laobian.Blog.Log
{
    public abstract class LogHostService : BackgroundService
    {
        protected readonly ILogger Logger;

        protected DateTime LastFlushAt;
        protected IEmailEmitter EmailEmitter;

        private readonly IAzureBlobClient _azureBlobClient;
        private readonly ConcurrentDictionary<string, List<BlogLog>> _logs; // this is synced with store
        private readonly ConcurrentDictionary<string, List<BlogLog>> _pendingLogs; // this is only exists in memory

        protected LogHostService(ILogger logger, IAzureBlobClient azureClient, IEmailEmitter emailEmitter)
        {
            Logger = logger;
            _azureBlobClient = azureClient;
            EmailEmitter = emailEmitter;
            LastFlushAt = DateTime.UtcNow;
            _logs = new ConcurrentDictionary<string, List<BlogLog>>();
            _pendingLogs = new ConcurrentDictionary<string, List<BlogLog>>();
        }

        protected abstract string GetBaseContainerName();

        // we always return copy of In-Memory logs, so that new logs will allow to add at the same time
        protected ConcurrentDictionary<string, List<BlogLog>> GetLogs()
        {
            var logs = new ConcurrentDictionary<string, List<BlogLog>>();
            foreach (var log in _logs.ToList())
            {
                logs.TryAdd(log.Key, log.Value.ToList());
            }

            return logs;
        }

        // we always return copy of In-Memory logs, so that new logs will allow to add at the same time
        protected ConcurrentDictionary<string, List<BlogLog>> GetPendingLogs()
        {
            var logs = new ConcurrentDictionary<string, List<BlogLog>>();
            foreach (var log in _pendingLogs.ToList())
            {
                logs.TryAdd(log.Key, log.Value.ToList());
            }

            return logs;
        }

        // only add to pending buffer
        protected void Add(string blobName, BlogLog log)
        {
            if (!blobName.StartsWith($"{GetBaseContainerName()}/"))
            {
                blobName = $"{GetBaseContainerName()}/{blobName}"; // a general fix if forget
            }

            if (!_pendingLogs.TryGetValue(blobName, out var logs) || logs == null)
            {
                logs = new List<BlogLog>();
                _pendingLogs.TryAdd(blobName, logs);
            }

            logs.Add(log);
        }

        protected virtual async Task InitAsync()
        {
            _logs.Clear();

            foreach (var data in await _azureBlobClient.ListAsync(
                BlobContainer.Private, 
                BlobNameProvider.Normalize(GetBaseContainerName())))
            {
                using (data.Stream)
                {
                    var logs = SerializeHelper.FromProtobuf<List<BlogLog>>(data.Stream);
                    logs = logs ?? new List<BlogLog>();
                    _logs.TryAdd(data.BlobName, logs);
                }
            }
        }

        protected virtual void Flush(ConcurrentDictionary<string, List<BlogLog>> pendingLogs = null)
        {
            if (pendingLogs == null)
            {
                pendingLogs = GetPendingLogs(); // get copy of pending buffer
            }
            var logs = GetLogs();

            // merge to logs
            foreach (var pendingLog in pendingLogs)
            {
                logs.AddOrUpdate(pendingLog.Key, pendingLog.Value, (key, existingLogs) =>
                {
                    existingLogs.AddRange(pendingLog.Value);
                    return existingLogs;
                });

                _logs.AddOrUpdate(pendingLog.Key, pendingLog.Value, (key, existingLogs) =>
                {
                    existingLogs.AddRange(pendingLog.Value);
                    return existingLogs;
                });
            }

            Parallel.ForEach(logs, async item =>
            {
                await _azureBlobClient.UploadAsync(BlobContainer.Private, item.Key, item.Value);
            });

            // remove item from pending logs
            foreach (var log in pendingLogs)
            {
                if (_pendingLogs.ContainsKey(log.Key))
                {
                    var values = _pendingLogs[log.Key];
                    values?.RemoveAll(vs => log.Value.Contains(vs));
                }
            }

            LastFlushAt = DateTime.UtcNow;
        }
    }
}
