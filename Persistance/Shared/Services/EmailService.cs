using Microsoft.Extensions.Options;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Persistance.Shared.Settings;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_mailSettings.EmailFrom, _mailSettings.DisplayName);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtp = new SmtpClient(_mailSettings.SmtpHost, _mailSettings.SmtpPort);
                smtp.Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                smtp.EnableSsl = true;

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
            }
        }
    }
}