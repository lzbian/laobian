using System.IO;
using System.Reflection;

namespace Laobian.Common.Notification
{
    /// <summary>
    /// Templates for email notification
    /// </summary>
    public class EmailTemplateProvider
    {
        private static string _baseTemplate;

        public static string Get(NotifyType notifyType)
        {
            LoadBaseTemplate();
            return _baseTemplate;
        }

        private static void LoadBaseTemplate()
        {
            if (string.IsNullOrEmpty(_baseTemplate))
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Notification/email-template.txt");
                _baseTemplate = File.ReadAllText(path);
            }
        }
    }
}
