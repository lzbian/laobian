using Laobian.Jarvis.Model;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Laobian.Jarvis.Log;
using Laobian.Jarvis.Option;

namespace Laobian.Jarvis
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            await FileLogger.Default.StartAsync();

            await JarvisOut.VerbAsync($"App started with arguments: {string.Join(" ", args)}");

            await OptionDispatcher.ParseAsync(args);

            await JarvisOut.VerbAsync("App executed completed successfully");
            await FileLogger.Default.StopAsync();
        }
    }
}
