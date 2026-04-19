namespace Event.Application.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string emailSubject, string emailBody);

        public interface IPasswordGenerator
        {
            string Generate(int length = 12);
        }
        public interface IEmailService
        {
            Task SendEmailAsync(string to, string subject, string body);
        }
    }
}