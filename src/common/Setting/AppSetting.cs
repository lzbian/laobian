using System;

namespace Laobian.Common.Setting
{
    /// <summary>
    /// App Settings, they are not secrets
    /// </summary>
    public sealed class AppSetting
    {
        private static readonly Lazy<AppSetting> LazyDefault = new Lazy<AppSetting>(() => new AppSetting(), true);

        /// <summary>
        /// Singleton instance of <see cref="AppSetting"/>
        /// </summary>
        public static AppSetting Default => LazyDefault.Value;

        private AppSetting()
        {
        }

        /// <summary>
        /// Admin email address
        /// </summary>
        public string AdminEmail { get; set; } = "JerryBian@outlook.com";

        /// <summary>
        /// Admin full name
        /// </summary>
        public string AdminFullName { get; set; } = "Jerry Bian";

        /// <summary>
        /// Notification email address
        /// </summary>
        public string NotifyEmail { get; set; } = "notify@laobian.me";

        /// <summary>
        /// Notification name
        /// </summary>
        public string NotifyName { get; set; } = "notification";

        /// <summary>
        /// Blog name
        /// </summary>
        public string BlogName { get; set; } = "Jerry Bian's Blog";

        /// <summary>
        /// Pending buffer size for visit logs
        /// </summary>
        public int VisitLogBufferSize { get; set; } = 30;

        /// <summary>
        /// Flush interval for visit logs
        /// </summary>
        public TimeSpan VisitLogFlushInterval { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Pending buffer size for status logs
        /// </summary>
        public int StatusLogBufferSize { get; set; } = 10;

        /// <summary>
        /// Flush interval for status logs
        /// </summary>
        public TimeSpan StatusLogFlushInterval { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Admin chinese name
        /// </summary>
        public string ChineseName { get; set; } = "卞良忠";

        /// <summary>
        /// Blog description
        /// </summary>
        public string BlogDescription { get; set; } = "个人博客，记录所思所得。";

    }
}
