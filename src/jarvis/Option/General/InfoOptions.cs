using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Common.Config;
using Laobian.Jarvis.Model;

namespace Laobian.Jarvis.Option.General
{
    [Verb("info", HelpText = "Display Jarvis information.")]
    public class InfoOptions : Options
    {
        protected override async Task HandleInternalAsync()
        {
            await JarvisOut.InfoAsync("Local setting location:\t\t\t{0}", AppConfig.Default.GetLocalLocation());

            await JarvisOut.InfoAsync("Installed location:\t\t\t{0}", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        }

        protected override bool IsAzureStorageConnectionValid()
        {
            return true;
        }
    }
}
