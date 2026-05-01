using events.domain.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Event.Application.IServices; 
namespace Event.Application.Services
{
    public class BookingReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BookingReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepo>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var from = DateTime.UtcNow.AddDays(7);
                var to = DateTime.UtcNow.AddDays(9);

                var bookings = await bookingRepo.GetBookingsForReminderAsync(from, to);

                foreach (var booking in bookings)
                {
                    if (booking.ReminderSent)
                        continue;

                    await emailService.SendEmailAsync(
                        booking.User.Email,
                        "Reminder: Trial Event",
                        $@"
                    <p>Dear {booking.User.FirstName},</p>

                    <p>Your trial event will take place in 2 days.</p>

                    <p>Venue: {booking.Venue.Name}</p>

                    <p>Best regards,<br/>Events Team</p>
                    "
                    );

                    booking.MarkReminderSent();
                }

                await bookingRepo.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
