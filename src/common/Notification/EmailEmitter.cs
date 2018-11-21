using System;
using System.Net;
using System.Threading.Tasks;
using Laobian.Common.Base;
using Laobian.Common.Config;
using Laobian.Common.Setting;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Laobian.Common.Notification
{
    /// <summary>
    /// Implementation of <see cref="IEmailEmitter"/>
    /// </summary>
    public class EmailEmitter : IEmailEmitter
    {
        #region Implementation of IEmailEmitter

        public async Task<bool> EmitErrorAsync(string htmlMessage, Exception exception = null)
        {
            if (exception != null)
            {
                htmlMessage += "<p>Full details:</p>";
                htmlMessage += $"<div><pre>{exception}</pre></div>";
            }

            var message = string.Format(EmailTemplateProvider.Get(NotifyType.ErrorReport), htmlMessage, DateTime.UtcNow.ToChinaTime().ToDateAndTime());
            return await EmitAsync("Please check ERROR!", message);
        }

        public async Task<bool> EmitHealthyAsync(string htmlMessage)
        {
            var message = string.Format(EmailTemplateProvider.Get(NotifyType.HealthyReport), htmlMessage, DateTime.UtcNow.ToChinaTime().ToDateAndTime());
            return await EmitAsync("Please check status ...", message);
        }

        public async Task<bool> EmitInfoAsync(string htmlMessage)
        {
            var message = string.Format(EmailTemplateProvider.Get(NotifyType.InfoReport), htmlMessage, DateTime.UtcNow.ToChinaTime().ToDateAndTime());
            return await EmitAsync("New activity...", message);
        }

        #endregion

        private async Task<bool> EmitAsync(string subject, string html)
        {
            if (string.IsNullOrEmpty(AppConfig.Default.SendGridKey) || string.IsNullOrEmpty(AppSetting.Default.AdminEmail))
            {
                return false;
            }

            var client = new SendGridClient(AppConfig.Default.SendGridKey);
            var from = new EmailAddress(AppSetting.Default.NotifyEmail, AppSetting.Default.NotifyName);
            var to = new EmailAddress(AppSetting.Default.AdminEmail, AppSetting.Default.AdminFullName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, html, html);
            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
