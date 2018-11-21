using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Common.Base;
using Laobian.Common.Config;
using Laobian.Jarvis.Model;

namespace Laobian.Jarvis.Option.General
{
    [Verb("config", HelpText = "Config apps, all of these settings are secrets.")]
    public class ConfigOptions : Options
    {
        [Option('n', "name", HelpText = "Specify name of configuration.")]
        public string Name { get; set; }

        [Option('v', "value", HelpText = "Specify value of configuration.")]
        public string Value { get; set; }

        protected override async Task HandleInternalAsync()
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Value))
            {
                await JarvisOut.InfoAsync("Following all configurations.");

                // list all existing configurations
                foreach (var propertyInfo in typeof(AppConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var name = propertyInfo.Name;
                    var value = propertyInfo.GetValue(AppConfig.Default);
                    await JarvisOut.InfoAsync("{0}\t{1}", name, value);
                }

                return;
            }

            var prop = typeof(AppConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(ps => ps.Name.EqualsIgnoreCase(Name));
            if (prop == null)
            {
                await JarvisOut.ErrorAsync($"Invalid config name {Name}");
                return;
            }

            prop.SetValue(AppConfig.Default, Value);
            AppConfig.Default.StoreToLocalConfig();
            await JarvisOut.InfoAsync("Configuration {0} set.", Name);
        }

        protected override bool IsAzureStorageConnectionValid()
        {
            return true;
        }
    }
}
