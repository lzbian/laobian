using Laobian.Common.Azure;
using Laobian.Common.Notification;
using Laobian.Common.Setting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Laobian.Common.Base;

namespace Laobian.Blog.Log
{
    public class StatusLogHostService : LogHostService
    {
        private const string SubContainerName = "status";
        private readonly string _emailTemplate;

        public StatusLogHostService(
            ILogger logger,
            IEmailEmitter emailEmitter,
            IAzureBlobClient azureClient) : base(logger, azureClient, emailEmitter)
        {
            _emailTemplate = @"<p>Status code: {0}, generated at {1}.</p><table role='presentation' border='0' cellpadding='0' cellspacing='0'>
                          <tbody>
                            <tr>
                              <td align='left'>
                                <table role='presentation' border='1' cellpadding='1' cellspacing='1'>
								  <thead><tr><th>URL</th><th>IP Address</th><th>Time</th><th>User Agent</th></tr></thead>
                                  <tbody>
									{2}
									
                                  </tbody>
                                </table>
                              </td>
                            </tr>
                          </tbody>
                        </table>";
            Logger.NewStatusLog += (sender, args) =>
            {
                var blobName = BlobNameProvider.Normalize($"{SubContainerName}/{args.StatusCode}");
                Add(blobName, args.Log);
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var logs = GetPendingLogs();
                var logsCount = logs.SelectMany(ls => ls.Value).Count();
                if (logsCount > 0 && (logsCount > AppSetting.Default.StatusLogBufferSize ||
                    DateTime.UtcNow - LastFlushAt > AppSetting.Default.StatusLogFlushInterval))
                {
                    await ExecuteInternalAsync(logs);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitAsync(SubContainerName);
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await ExecuteInternalAsync(GetPendingLogs());
            await EmailEmitter.EmitHealthyAsync("<p>Status Logs Flushed due to service is stopping...</p>");
            await base.StopAsync(cancellationToken);
        }

        private async Task ExecuteInternalAsync(ConcurrentDictionary<string, List<BlogLog>> logs)
        {
            try
            {
                Flush(logs);
            }
            catch (Exception ex)
            {
                await EmailEmitter.EmitErrorAsync($"<p>Status Log host service error: {ex.Message}</p>", ex);
            }

            var messages = new StringBuilder();
            foreach (var log in logs)
            {
                var rows = new StringBuilder();
                foreach (var blogLog in log.Value.OrderByDescending(l => l.When))
                {
                    var columns = new StringBuilder();
                    columns.AppendFormat("<td>{0}</td>", blogLog.FullUrl);
                    columns.AppendFormat("<td>{0}</td>", blogLog.RemoteIp);
                    columns.AppendFormat("<td>{0}</td>", blogLog.When.ToChinaTime().ToIso8601());
                    columns.AppendFormat("<td>{0}</td>", blogLog.UserAgent);
                    rows.AppendFormat("<tr>{0}</tr>", columns);
                }

                messages.AppendFormat(_emailTemplate, log.Key, DateTime.UtcNow.ToChinaTime().ToIso8601(), rows);
            }

            if (messages.Length > 0)
            {
                await EmailEmitter.EmitHealthyAsync(messages.ToString());
            }
        }
    }
}
