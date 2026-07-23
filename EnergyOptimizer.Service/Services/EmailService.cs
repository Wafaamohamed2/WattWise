using EnergyOptimizer.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EnergyOptimizer.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var host = _config["Smtp:Host"];
                var portStr = _config["Smtp:Port"];
                var username = _config["Smtp:Username"];
                var password = _config["Smtp:Password"];
                var fromName = _config["Smtp:FromName"] ?? "WattWise Support";

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || username == "YOUR_SMTP_USERNAME")
                {
                    _logger.LogWarning("SMTP is not fully configured in appsettings. Logged email for {Email} with subject '{Subject}'.", toEmail, subject);
                    return;
                }

                int port = int.TryParse(portStr, out var p) ? p : 587;

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, username));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                var enableSsl = bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) && ssl;
                var socketOption = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

                await smtp.ConnectAsync(host, port, socketOption);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Successfully sent email to {Email} with subject '{Subject}'.", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            }
        }
    }
}
