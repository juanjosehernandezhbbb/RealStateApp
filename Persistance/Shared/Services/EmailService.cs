using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Persistance.Shared.Settings;
using System;
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
                // 1. Crear el mensaje
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_mailSettings.EmailFrom);
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                // 2. Construir el cuerpo en HTML
                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody(); // <-- Corrección aquí

                // 3. Conectar y enviar usando MailKit
                using var smtp = new MailKit.Net.Smtp.SmtpClient();

                // Conectar con STARTTLS en el puerto 587
                await smtp.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);

                // Autenticar con tu correo real y la contraseña de aplicación
                await smtp.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);

                // Enviar el correo
                await smtp.SendAsync(email);

                // Desconectar limpiamente
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Si ocurre un error, lo lanzará para que lo veamos en Visual Studio
                throw new Exception($"Error SMTP con MailKit: {ex.Message}", ex);
            }
        }
    }
}