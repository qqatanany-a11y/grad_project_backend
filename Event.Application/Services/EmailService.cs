using Event.Application.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Event.Application.Services
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

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var portValue = smtpSettings["Port"];
            var user = smtpSettings["User"];
            var password = smtpSettings["Pass"];
            var securityValue = smtpSettings["Security"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portValue) ||
                !int.TryParse(portValue, out var port) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("SMTP settings are not configured correctly.");
            }

            var normalizedPassword = NormalizePassword(host, password);
            var message = BuildMessage(user, to, subject, body);
            var socketOptions = ResolveSocketOptions(port, securityValue);

            using var client = new SmtpClient();

            try
            {
                client.Timeout = 30000;

                await client.ConnectAsync(host, port, socketOptions);
                await client.AuthenticateAsync(user, normalizedPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient} using {Host}:{Port}", to, host, port);
                throw new InvalidOperationException("Email delivery failed. Check SMTP settings or network access.", ex);
            }
        }

        private static MimeMessage BuildMessage(string from, string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder
            {
                HtmlBody = body
            }.ToMessageBody();

            return message;
        }

        private static SecureSocketOptions ResolveSocketOptions(int port, string? configuredValue)
        {
            if (!string.IsNullOrWhiteSpace(configuredValue) &&
                Enum.TryParse<SecureSocketOptions>(configuredValue, ignoreCase: true, out var configuredOption))
            {
                return configuredOption;
            }

            return port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };
        }

        private static string NormalizePassword(string host, string password)
        {
            if (host.Contains("gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return password.Replace(" ", string.Empty);
            }

            return password;
        }
    }
}
