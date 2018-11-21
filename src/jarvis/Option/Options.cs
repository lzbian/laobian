using Laobian.Jarvis.Model;
using System;
using System.Threading.Tasks;
using Laobian.Common.Config;

namespace Laobian.Jarvis.Option
{
    /// <summary>
    /// Base for all options
    /// </summary>
    public abstract class Options
    {
        protected Options() { }

        /// <summary>
        /// Handle commands
        /// </summary>
        /// <returns>Task</returns>
        public async Task HandleAsync()
        {
            try
            {
                if (!IsAzureStorageConnectionValid())
                {
                    await JarvisOut.ErrorAsync("No azure storage connection was configured, please use 'config -n AzureStorageConnectionString -v XXX' to get it fixed.");
                    return;
                }

                await HandleInternalAsync();
                await JarvisOut.VerbAsync("Command handler completed");
            }
            catch(Exception ex)
            {
                await JarvisOut.ErrorAsync($"Fail - {ex.Message}", ex);
            }
        }

        protected abstract Task HandleInternalAsync();

        protected virtual bool IsAzureStorageConnectionValid()
        {
            return !string.IsNullOrEmpty(AppConfig.Default.AzureStorageConnection);
        }
    }
}
