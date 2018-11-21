using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laobian.Common;
using Laobian.Common.Azure;
using Laobian.Common.Base;
using Laobian.Common.Blog;
using Laobian.Common.Notification;
using Laobian.Common.Setting;

namespace Laobian.Blog.Log
{
    public class VisitLogHostService : LogHostService
    {
        private readonly IPostRepository _postRepository;

        public VisitLogHostService(
            ILogger logger, 
            IEmailEmitter emailEmitter,
            IAzureBlobClient azureClient,
            IPostRepository postRepository) : base(logger, azureClient, emailEmitter)
        {
            _postRepository = postRepository;
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

        protected override async Task InitAsync(string subContainerName = null)
        {
            await base.InitAsync(subContainerName);
            await SetPostsVisitCountAsync();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitAsync();
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await ExecuteInternalAsync();
            await EmailEmitter.EmitHealthyAsync("<p>Visit Logs Flushed due to service is stopping...</p>");
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

        private async Task SetPostsVisitCountAsync()
        {
            var logs = GetLogs();
            var posts = await _postRepository.GetPostsAsync();
            foreach (var log in logs) // same group logs own same ID.
            {
                var postPrefix = BlobNameProvider.Normalize($"{BaseContainerName}/{VisitLogCategory.Post}/");
                if (log.Key.StartsWith(postPrefix))
                {
                    var post = posts.FirstOrDefault(ps => ps.Id.Normal() == log.Key.Replace(postPrefix, string.Empty));
                    post?.SetVisitCount(log.Value.Count);
                }
            }

            _postRepository.UpdatePostsCache(posts);
            SystemState.VisitLogs = logs.Count;
        }

        private async Task ExecuteInternalAsync()
        {
            try
            {
                // flush all logs to Azure Blob
                Flush();

                // after flush, we also need to update Posts visit count in cache
                await SetPostsVisitCountAsync();
            }
            catch (Exception ex)
            {
                await EmailEmitter.EmitErrorAsync($"<p>Visit Log host service error: {ex.Message}</p>", ex);
            }
        }
    }
}
