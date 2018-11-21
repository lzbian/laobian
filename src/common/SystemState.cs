using System;
using Laobian.Common.Base;

namespace Laobian.Common
{
    public static class SystemState
    {
        public static TimeSpan PostCacheWindow => TimeSpan.FromHours(8);

        public static DateTime StartupTime { get; set; }

        public static DateTime PostCacheTime { get; set; }

        public static int PublishedPosts { get; set; }

        public static int VisitLogs { get; set; }


        public static string GetStartupStatistic()
        {
            return $"Startup at {StartupTime.ToChinaTime().ToIso8601()}, it has been up for {DateTime.UtcNow - StartupTime}.";
        }

        public static string GetPostStatistic()
        {
            return $"BlogPost cached at {PostCacheTime.ToChinaTime().ToIso8601()}, next refresh will happen after {PostCacheTime + PostCacheWindow - DateTime.UtcNow}.";
        }
    }
}
