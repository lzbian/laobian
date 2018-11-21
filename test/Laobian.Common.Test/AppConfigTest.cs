using System;
using System.IO;
using System.Threading.Tasks;
using Laobian.Common.Base;
using Laobian.Common.Config;
using Xunit;

namespace Laobian.Common.Test
{
    public class AppConfigTest
    {
        [Fact]
        public void NoSourceDefined_PropertyAsDefault()
        {
            Assert.True(string.IsNullOrEmpty(AppConfig.Default.AzureStorageConnection));
            Assert.True(string.IsNullOrEmpty(AppConfig.Default.SendGridKey));
        }

        [Fact]
        public void EnvironmentSourceDefined()
        {
            const string azureStorageConnection = "azure storage connection";
            const string sendGridKey = "send grid key";
            Environment.SetEnvironmentVariable(nameof(AppConfig.AzureStorageConnection), azureStorageConnection);
            Environment.SetEnvironmentVariable(nameof(AppConfig.SendGridKey), sendGridKey);

            Assert.Equal(AppConfig.Default.AzureStorageConnection, azureStorageConnection);
            Assert.Equal(AppConfig.Default.SendGridKey, sendGridKey);
        }

        [Fact]
        public void LocalConfigSourceDefined()
        {
            const string azureStorageConnection = "azure storage connection";
            const string sendGridKey = "send grid key";
            Environment.SetEnvironmentVariable(nameof(AppConfig.AzureStorageConnection), azureStorageConnection);
            Environment.SetEnvironmentVariable(nameof(AppConfig.SendGridKey), sendGridKey);

            var localConfigFile = AppConfig.Default.GetLocalConfigFile();
            var localConfigFileBackup = localConfigFile + ".backup";
            try
            {
                AppConfig.Default.AzureStorageConnection = "con1";
                AppConfig.Default.SendGridKey = "key1";
                
                if (File.Exists(localConfigFile))
                {
                    // we need to backup exisiting file
                    File.Copy(localConfigFile, localConfigFileBackup, true);
                }

                AppConfig.Default.StoreToLocalConfig();
                Assert.Equal("con1", AppConfig.Default.AzureStorageConnection);
                Assert.Equal("key1", AppConfig.Default.SendGridKey);
            }
            finally
            {
                if (File.Exists(localConfigFileBackup))
                {
                    File.Copy(localConfigFileBackup, localConfigFile, true);
                }
                else
                {
                    File.Delete(localConfigFile);
                }
            }
        }

        [Fact]
        public async Task LocalConfigSourceDefined_FileChanged()
        {
            const string azureStorageConnection = "azure storage connection";
            const string sendGridKey = "send grid key";

            var localConfigFile = AppConfig.Default.GetLocalConfigFile();
            var localConfigFileBackup = localConfigFile + ".backup";
            try
            {
                if (File.Exists(localConfigFile))
                {
                    // we need to backup exisiting file
                    File.Copy(localConfigFile, localConfigFileBackup, true);
                }

                AppConfig.Default.AzureStorageConnection = azureStorageConnection;
                AppConfig.Default.SendGridKey = sendGridKey;
                AppConfig.Default.StoreToLocalConfig();
                Assert.Equal(azureStorageConnection, AppConfig.Default.AzureStorageConnection);
                Assert.Equal(sendGridKey, AppConfig.Default.SendGridKey);

                AppConfig.Default.AzureStorageConnection = "con1";
                AppConfig.Default.SendGridKey = "key1";
                var json2 = SerializeHelper.ToJson(AppConfig.Default);
                AppConfig.Default.AzureStorageConnection = azureStorageConnection;
                AppConfig.Default.SendGridKey = sendGridKey;
                File.WriteAllText(localConfigFile, json2.EncodeAsBase64());
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                Assert.Equal("con1", AppConfig.Default.AzureStorageConnection);
                Assert.Equal("key1", AppConfig.Default.SendGridKey);
            }
            finally
            {
                if (File.Exists(localConfigFileBackup))
                {
                    File.Copy(localConfigFileBackup, localConfigFile, true);
                }
                else
                {
                    File.Delete(localConfigFile);
                }
            }
        }
    }
}
