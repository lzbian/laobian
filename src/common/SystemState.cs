using System;
using System.Collections.Concurrent;
using Laobian.Common.Base;
using Laobian.Common.Setting;

namespace Laobian.Common
{
    public static class SystemState
    {
        public static DateTime StartupTime { get; set; }

        public static DateTime PostReloadedAt { get; set; }

        public static int PublishedPosts { get; set; }

        public static int VisitLogs { get; set; }

        public static int StatusLogs { get; set; }

        public static ConcurrentDictionary<Guid, int> PostsVisitCount { get; } = new ConcurrentDictionary<Guid, int>();

        public static string GetStartupStatistic()
        {
            return $"Startup at {StartupTime.ToChinaTime().ToIso8601()}, it has been up for {DateTime.UtcNow - StartupTime}.";
        }

        public static string GetPostStatistic()
        {
            return $"BlogPost loaded at {PostReloadedAt.ToChinaTime().ToIso8601()}, next refresh will happen after {PostReloadedAt + AppSetting.Default.BlogPostReloadInterval - DateTime.UtcNow}.";
        }
    }
}
