using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepo _bookingRepo;
        private readonly IVenueRepo _venueRepo;

        public BookingService(IBookingRepo bookingRepo, IVenueRepo venueRepo)
        {
            _bookingRepo = bookingRepo;
            _venueRepo = venueRepo;
        }

        public async Task CreateBooking(int userId, CreateBookingDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(dto.VenueId);

            if (venue == null)
                throw new Exception("Venue not found");

            if (dto.EndTime <= dto.StartTime)
                throw new Exception("End time must be after start time");

            var bookingDateUtc = dto.Date.Kind == DateTimeKind.Utc
                ? dto.Date
                : DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);

            var existing = await _bookingRepo.GetByVenueAndDate(dto.VenueId, bookingDateUtc);

            foreach (var b in existing)
            {
                if (dto.StartTime < b.EndTime && dto.EndTime > b.StartTime)
                    throw new Exception("Time not available");
            }

            var booking = new Booking(
                dto.VenueId,
                userId,
                bookingDateUtc,
                dto.StartTime,
                dto.EndTime,
                dto.GuestsCount,
                venue.MinimalPrice
            );

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();
        }
        public async Task<List<BookingDto>> GetMyBookings(int userId)
        {
            var bookings = await _bookingRepo.GetUserBookings(userId);

            return bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                VenueName = b.Venue.Name,
                Date = b.BookingDate,
                Time = $"{b.StartTime} - {b.EndTime}",
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString()
            }).ToList();
        }

        public async Task<List<BookingDto>> GetOwnerBookings(int ownerId)
        {
            var bookings = await _bookingRepo.GetOwnerBookings(ownerId);

            return bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                VenueName = b.Venue.Name,
                Date = b.BookingDate,
                Time = $"{b.StartTime} - {b.EndTime}",
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString()
            }).ToList();
        }

        public async Task Approve(int bookingId, int ownerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            if (booking.Venue.Company.UserId != ownerId)
                throw new Exception("Not allowed");

            booking.Approve(ownerId);
            await _bookingRepo.SaveChangesAsync();
        }

        public async Task Reject(int bookingId, int ownerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            if (booking.Venue.Company.UserId != ownerId)
                throw new Exception("Not allowed");

            booking.Reject(ownerId);
            await _bookingRepo.SaveChangesAsync();
        }
    }
}