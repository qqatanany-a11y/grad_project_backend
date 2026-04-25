using Event.Application.IServices;
using System.Net;
using System.Net.Mail;

namespace Event.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public EmailService(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            using var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]))
            {
                Credentials = new NetworkCredential(smtpSettings["User"], smtpSettings["Pass"]),
                EnableSsl = true
            };
            var mailMessage = new MailMessage(smtpSettings["User"], to, subject, body);
            await client.SendMailAsync(mailMessage);
        }
    }
}
