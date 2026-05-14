using Event.Application.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
            var smtpSettings = ResolveSmtpSettings();
            var normalizedPassword = NormalizePassword(smtpSettings.Host, smtpSettings.Password);
            var message = BuildMessage(smtpSettings, to, subject, body);
            var socketOptions = ResolveSocketOptions(smtpSettings.Port, smtpSettings.Security);

            using var client = new SmtpClient();

            try
            {
                client.Timeout = 30000;

                await client.ConnectAsync(smtpSettings.Host, smtpSettings.Port, socketOptions);
                await client.AuthenticateAsync(smtpSettings.User, normalizedPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email to {Recipient} using {Host}:{Port}",
                    to,
                    smtpSettings.Host,
                    smtpSettings.Port);
                throw new InvalidOperationException("Email delivery failed. Check SMTP settings or network access.", ex);
            }
        }

        private MimeMessage BuildMessage(ResolvedSmtpSettings smtpSettings, string to, string subject, string body)
        {
            if (!MailboxAddress.TryParse(smtpSettings.From, out var fromAddress))
            {
                throw new InvalidOperationException(
                    "SMTP sender address is invalid. Set SmtpSettings:From or SMTP_FROM to a valid email address.");
            }

            if (!MailboxAddress.TryParse(to, out var toAddress))
            {
                throw new InvalidOperationException("Recipient email address is invalid.");
            }

            var message = new MimeMessage();
            message.From.Add(string.IsNullOrWhiteSpace(smtpSettings.FromName)
                ? fromAddress
                : new MailboxAddress(smtpSettings.FromName, fromAddress.Address));
            message.To.Add(toAddress);
            message.Subject = subject;
            message.Body = new BodyBuilder
            {
                HtmlBody = body
            }.ToMessageBody();

            return message;
        }

        private ResolvedSmtpSettings ResolveSmtpSettings()
        {
            var missingSettings = new List<string>();
            var host = GetSettingValue("SmtpSettings:Host", "SMTP_HOST", "MAIL_HOST");
            var portValue = GetSettingValue("SmtpSettings:Port", "SMTP_PORT", "MAIL_PORT");
            var user = GetSettingValue("SmtpSettings:User", "SMTP_USER", "SMTP_USERNAME", "MAIL_USERNAME", "EMAIL_HOST_USER");
            var password = GetSettingValue("SmtpSettings:Pass", "SMTP_PASS", "SMTP_PASSWORD", "MAIL_PASSWORD", "EMAIL_HOST_PASSWORD");
            var security = GetSettingValue("SmtpSettings:Security", "SMTP_SECURITY", "MAIL_SECURITY");
            var from = GetSettingValue("SmtpSettings:From", "SMTP_FROM", "MAIL_FROM", "EMAIL_FROM");
            var fromName = GetSettingValue("SmtpSettings:FromName", "SMTP_FROM_NAME", "MAIL_FROM_NAME", "EMAIL_FROM_NAME");

            if (string.IsNullOrWhiteSpace(host))
            {
                missingSettings.Add("Host");
            }

            if (string.IsNullOrWhiteSpace(portValue))
            {
                missingSettings.Add("Port");
            }

            if (string.IsNullOrWhiteSpace(user))
            {
                missingSettings.Add("User");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                missingSettings.Add("Pass");
            }

            if (missingSettings.Count > 0)
            {
                throw new InvalidOperationException(
                    $"SMTP settings are not configured correctly. Missing: {string.Join(", ", missingSettings)}.");
            }

            if (!int.TryParse(portValue, out var port))
            {
                throw new InvalidOperationException("SMTP port is invalid.");
            }

            var senderAddress = !string.IsNullOrWhiteSpace(from) ? from : user;

            return new ResolvedSmtpSettings(
                host!,
                port,
                user!,
                password!,
                security,
                senderAddress!,
                fromName);
        }

        private static SecureSocketOptions ResolveSocketOptions(int port, string? configuredValue)
        {
            if (!string.IsNullOrWhiteSpace(configuredValue) &&
                TryMapSocketOption(configuredValue, out var configuredOption))
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

        private static bool TryMapSocketOption(string configuredValue, out SecureSocketOptions socketOptions)
        {
            var normalizedValue = configuredValue
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal);

            switch (normalizedValue.ToLowerInvariant())
            {
                case "ssl":
                case "sslonconnect":
                    socketOptions = SecureSocketOptions.SslOnConnect;
                    return true;

                case "tls":
                case "starttls":
                    socketOptions = SecureSocketOptions.StartTls;
                    return true;

                case "starttlswhenavailable":
                    socketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                    return true;

                case "none":
                    socketOptions = SecureSocketOptions.None;
                    return true;

                case "auto":
                    socketOptions = SecureSocketOptions.Auto;
                    return true;

                default:
                    return Enum.TryParse(configuredValue, ignoreCase: true, out socketOptions);
            }
        }

        private static string NormalizePassword(string host, string password)
        {
            if (host.Contains("gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return password.Replace(" ", string.Empty);
            }

            return password;
        }

        private string? GetSettingValue(params string[] keys)
        {
            foreach (var key in keys)
            {
                var value = _config[key];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        private sealed record ResolvedSmtpSettings(
            string Host,
            int Port,
            string User,
            string Password,
            string? Security,
            string From,
            string? FromName);
    }
}
