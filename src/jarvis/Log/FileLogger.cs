using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Laobian.Common.Base;
using Laobian.Common.Config;

namespace Laobian.Jarvis.Log
{
    /// <summary>
    /// File logger, which writes logs to local files
    /// </summary>
    public class FileLogger
    {
        private static FileLogger _fileLogger;
        private readonly string _logLocation;
        private bool _started;
        private bool _stopped;

        private FileLogger()
        {
            _logLocation = Path.Combine(AppConfig.Default.GetLocalLocation(), "log");
            Directory.CreateDirectory(_logLocation);
            _logLocation = Path.Combine(_logLocation, DateTime.UtcNow.ToChinaTime().ToDate() + ".txt");
        }

        /// <summary>
        /// Singleton instance of <see cref="FileLogger"/>
        /// </summary>
        public static FileLogger Default => _fileLogger ?? (_fileLogger = new FileLogger());

        /// <summary>
        /// Start logging
        /// </summary>
        /// <returns>Task</returns>
        public async Task StartAsync()
        {
            if (!_started)
            {
                await File.AppendAllTextAsync(_logLocation, $"{Environment.NewLine}**********{Environment.NewLine}", Encoding.UTF8);
                await File.AppendAllTextAsync(_logLocation, Format("Jarvis started."), Encoding.UTF8);
                _started = true;
            }
        }

        /// <summary>
        /// Stop logging
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            if (!_stopped)
            {
                await File.AppendAllTextAsync(_logLocation, Format("Jarvis stopped."), Encoding.UTF8);
                await File.AppendAllTextAsync(_logLocation, $"{Environment.NewLine}**********{Environment.NewLine}", Encoding.UTF8);
                _stopped = true;
            }
        }

        /// <summary>
        /// Log message
        /// </summary>
        /// <param name="message">The given message</param>
        /// <returns>Task</returns>
        public async Task LogAsync(string message)
        {
            await File.AppendAllTextAsync(_logLocation, Format(message), Encoding.UTF8);
        }

        private string Format(string message)
        {
            return $"{DateTime.UtcNow.ToChinaTime().ToIso8601()}\t\t{message}{Environment.NewLine}";
        }
    }
}
