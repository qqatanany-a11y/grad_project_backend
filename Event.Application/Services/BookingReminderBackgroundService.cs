using Event.Application.IServices;
using events.domain.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Event.Application.Services
{
    public class BookingReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingReminderBackgroundService> _logger;

        public BookingReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Reminder Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepo>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var now = DateTime.UtcNow;
                    var targetDate = DateTime.UtcNow.Date.AddDays(9);

                    var fromUtc = targetDate;
                    var toUtc = targetDate.AddDays(1);

                    var bookings = await bookingRepo.GetBookingsForReminderAsync(fromUtc, toUtc);
                    foreach (var booking in bookings)
                    {
                        var subject = "Brova Reminder";

                        var brovaDate = booking.BookingDate.Date.AddDays(-7);

                        var body = $@"
                        <h3>Hello {booking.User.FirstName},</h3>

                        <p>This is a reminder for your upcoming Brova.</p>

                        <p>
                        Your booking at <strong>{booking.Venue.Name}</strong> is scheduled for 
                        <strong>{booking.BookingDate:yyyy-MM-dd}</strong>.
                        </p>

                        <p>
                        Please note that your Brova is in <strong>2 days</strong>, on 
                        <strong>{brovaDate:yyyy-MM-dd}</strong>.
                        </p>

                        <p>
                        Booking time: <strong>{booking.StartTime} - {booking.EndTime}</strong>
                        </p>

                        <p>Thank you.</p>";

                        // 🔥 أهم سطرين
                        await emailService.SendEmailAsync(booking.User.Email, subject, body);

                        booking.MarkReminderSent();
                    }
                    if (bookings.Any())
                    {
                        await bookingRepo.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending booking reminders.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}