using Laobian.Common.Base;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Laobian.Common.Config
{
    /// <summary>
    /// App configuration, the secrets
    /// </summary>
    public sealed class AppConfig
    {
        private static readonly Lazy<AppConfig> LazyDefault = new Lazy<AppConfig>(() => new AppConfig(), true);

        private AppConfig()
        {
        }

        /// <summary>
        /// Singleton instance of <see cref="AppConfig"/>
        /// </summary>
        public static AppConfig Default
        {
            get
            {
                if (!LazyDefault.IsValueCreated)
                {
                    LazyDefault.Value.Load();
                }

                return LazyDefault.Value;
            }
        }

        /// <summary>
        /// Azure storage connection string
        /// </summary>
        public string AzureStorageConnection { get; set; }

        /// <summary>
        /// SendGrid API key
        /// </summary>
        public string SendGridKey { get; set; }

        /// <summary>
        /// Get location for local store
        /// </summary>
        /// <returns></returns>
        public string GetLocalLocation()
        {
            var localAppData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "laobian");
            Directory.CreateDirectory(localAppData);
            return localAppData;
        }

        /// <summary>
        /// Get file path for local configuration
        /// </summary>
        /// <returns></returns>
        public string GetLocalConfigFile()
        {
            return Path.Combine(GetLocalLocation(), "config");
        }

        /// <summary>
        /// Flush to local configuration
        /// </summary>
        public void StoreToLocalConfig()
        {
            var localConfigFile = GetLocalConfigFile();
            var json = SerializeHelper.ToJson(this);
            File.WriteAllText(localConfigFile, json.EncodeAsBase64(), Encoding.UTF8);
        }

        private void Load()
        {
            var localConfigFile = GetLocalConfigFile();
            var appConfig = LoadFromLocalConfig(localConfigFile);

            foreach (var propertyInfo in typeof(AppConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (appConfig == null || propertyInfo.GetValue(appConfig) == default)
                {
                    var environmentValue = Environment.GetEnvironmentVariable(propertyInfo.Name);
                    if (!string.IsNullOrEmpty(environmentValue))
                    {
                        propertyInfo.SetValue(this, Convert.ChangeType(environmentValue, propertyInfo.PropertyType));
                    }
                }
                else
                {
                    propertyInfo.SetValue(this, propertyInfo.GetValue(appConfig));
                }
            }
        }

        private AppConfig LoadFromLocalConfig(string configFile)
        {
            if (!File.Exists(configFile))
            {
                return null;
            }

            try
            {
                var base64 = File.ReadAllText(configFile, Encoding.UTF8);
                return SerializeHelper.FromJson<AppConfig>(base64.DecodeFromBase64());
            }
            catch
            {
                return null;
            }
        }
    }
}
