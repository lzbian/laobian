using System;
using System.Drawing;
using System.Threading.Tasks;
using Laobian.Jarvis.Log;
using Console = Colorful.Console;

namespace Laobian.Jarvis.Model
{
    /// <summary>
    /// Output stream for Jarvis
    /// </summary>
    public static class JarvisOut
    {
        /// <summary>
        /// Output as verbose message
        /// </summary>
        /// <param name="message">The given message</param>
        /// <returns>Task</returns>
        public static async Task VerbAsync(string message)
        {
            await FileLogger.Default.LogAsync(message);
        }

        /// <summary>
        /// Output as information message
        /// </summary>
        /// <param name="message">The given message</param>
        /// <returns>Task</returns>
        public static async Task InfoAsync(string message)
        {
            Console.WriteLine(message, Color.YellowGreen);
            await FileLogger.Default.LogAsync(message);
        }

        /// <summary>
        /// Output as information message
        /// </summary>
        /// <param name="format">Message format</param>
        /// <param name="args">Message arguments</param>
        /// <returns>Task</returns>
        public static async Task InfoAsync(string format, params object[] args)
        {
            Console.WriteLine(format, Color.YellowGreen, args);
            await FileLogger.Default.LogAsync(string.Format(format, args));
        }

        /// <summary>
        /// Output as error message
        /// </summary>
        /// <param name="message">The given message</param>
        /// <param name="ex">Exception object</param>
        /// <returns>Task</returns>
        public static async Task ErrorAsync(string message, Exception ex = null)
        {
            if(ex == null)
            {
                Console.WriteLine(message, Color.DarkRed);
                await FileLogger.Default.LogAsync(message);
                return;
            }

            Console.WriteLine($"{message}", Color.DarkRed);
            await FileLogger.Default.LogAsync($"{message}{ex}");
        }
    }
}
