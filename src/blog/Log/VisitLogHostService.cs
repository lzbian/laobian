using Laobian.Common;
using Laobian.Common.Azure;
using Laobian.Common.Base;
using Laobian.Common.Notification;
using Laobian.Common.Setting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Laobian.Blog.Log
{
    public class VisitLogHostService : LogHostService
    {
        public VisitLogHostService(
            ILogger logger,
            IEmailEmitter emailEmitter,
            IAzureBlobClient azureClient) : base(logger, azureClient, emailEmitter)
        {
            Logger.NewVisitLog += (sender, args) =>
            {
                var blobName = GetBlobName(args.Category, args.Log.Id.Normal());
                Add(blobName, args.Log);
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var logs = GetPendingLogs();
                var logsCount = logs.SelectMany(ls => ls.Value).Count();
                if (logsCount > 0 && (logsCount > AppSetting.Default.VisitLogBufferSize ||
                    DateTime.UtcNow - LastFlushAt > AppSetting.Default.VisitLogFlushInterval))
                {
                    await ExecuteInternalAsync();
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }

        protected override string GetBaseContainerName()
        {
            return BlobNameProvider.Normalize("log");
        }

        protected override async Task InitAsync()
        {
            await base.InitAsync();
            SetPostsVisitCount();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitAsync();
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await ExecuteInternalAsync();
            await base.StopAsync(cancellationToken);
        }

        private string GetBlobName(VisitLogCategory category, string id)
        {
            var name = category.ToString();
            if (category == VisitLogCategory.Post)
            {
                if (!Guid.TryParse(id, out _))
                {
                    throw new ArgumentException($"While category set to Post, the id: {id} must be GUID format.");
                }

                name = $"{name}/{id}";
            }

            return BlobNameProvider.Normalize(name);
        }

        private void SetPostsVisitCount()
        {
            var logs = GetLogs();
            SystemState.PostsVisitCount.Clear();
            foreach (var log in logs)
            {
                if (TryExtractPostId(log.Key, out var postId))
                {
                    SystemState.PostsVisitCount[postId] = log.Value.Count;
                }
            }

            SystemState.VisitLogs = logs.SelectMany(ls => ls.Value).Count();
        }

        private async Task ExecuteInternalAsync()
        {
            try
            {
                // flush all logs to Azure Blob
                Flush();

                // after flush, we also need to update Posts visit count in cache
                SetPostsVisitCount();
            }
            catch (Exception ex)
            {
                await EmailEmitter.EmitErrorAsync($"<p>Visit Log host service error: {ex.Message}</p>", ex);
            }
        }

        private bool TryExtractPostId(string blobName, out Guid postId)
        {
            var postPrefix = BlobNameProvider.Normalize($"{GetBaseContainerName()}/{VisitLogCategory.Post}/");
            var index = blobName.IndexOf(postPrefix, StringComparison.Ordinal);
            if (index >= 0)
            {
                var postIdString = blobName.Substring(index + postPrefix.Length);
                if (Guid.TryParse(postIdString, out postId))
                {
                    return true;
                }
            }

            postId = Guid.Empty;
            return false;
        }
    }
}
