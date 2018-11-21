using System;
using System.Threading.Tasks;

namespace Laobian.Common.Notification
{
    /// <summary>
    /// Notification emitter
    /// </summary>
    public interface INotifyEmitter
    {
        /// <summary>
        /// Notify information message
        /// </summary>
        /// <param name="htmlMessage">HTML format message</param>
        /// <returns>Success or not</returns>
        Task<bool> EmitInfoAsync(string htmlMessage);

        /// <summary>
        /// Notify error message
        /// </summary>
        /// <param name="htmlMessage">HTML format message</param>
        /// <param name="exception">Exception details</param>
        /// <returns>Success or not</returns>
        Task<bool> EmitErrorAsync(string htmlMessage, Exception exception = null);

        /// <summary>
        /// Notify healthy message
        /// </summary>
        /// <param name="htmlMessage">HTML format message</param>
        /// <returns>Success or not</returns>
        Task<bool> EmitHealthyAsync(string htmlMessage);
    }
}
