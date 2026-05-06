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
        private readonly IUserRepo _userRepo;
        private readonly IVenueAvailabilityRepo _venueAvailabilityRepo;
        private readonly IVenueServiceOptionRepo _venueServiceOptionRepo;
        private readonly IBookingSelectedServiceRepo _bookingSelectedServiceRepo;
        private readonly IEmailService _emailService;

        private static List<SelectedServiceResponseDto> MapSelectedServices(Booking booking)
        {
            return booking.SelectedServices.Select(x => new SelectedServiceResponseDto
            {
                VenueServiceOptionId = x.VenueServiceOptionId,
                ServiceName = x.VenueServiceOption?.Service?.Name ?? string.Empty,
                Price = x.Price
            }).ToList();
        }

        public BookingService(
            IBookingRepo bookingRepo,
            IVenueRepo venueRepo,
            IUserRepo userRepo,
            IVenueAvailabilityRepo venueAvailabilityRepo,
            IVenueServiceOptionRepo venueServiceOptionRepo,
            IBookingSelectedServiceRepo bookingSelectedServiceRepo,
            IEmailService emailService)
        {
            _bookingRepo = bookingRepo;
            _venueRepo = venueRepo;
            _userRepo = userRepo;
            _venueAvailabilityRepo = venueAvailabilityRepo;
            _venueServiceOptionRepo = venueServiceOptionRepo;
            _bookingSelectedServiceRepo = bookingSelectedServiceRepo;
            _emailService = emailService;
        }

        public async Task<CreateBookingResponseDto> CreateBooking(int userId, CreateBookingDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(dto.VenueId);

            if (venue == null)
                throw new Exception("Venue not found");

            if (!venue.IsActive)
                throw new Exception("This venue is currently inactive.");

            var bookingDateSource = dto.BookingDate ?? dto.Date;

            if (!bookingDateSource.HasValue)
                throw new Exception("Booking date is required.");

            if (dto.GuestsCount.HasValue && dto.GuestsCount.Value <= 0)
                throw new Exception("Guests count must be greater than zero.");

            if (dto.GuestsCount.HasValue && dto.GuestsCount.Value > venue.Capacity)
                throw new Exception("Guests count exceeds venue capacity.");

            var bookingDateValue = bookingDateSource.Value;
            var bookingDateUtc = bookingDateValue.Kind == DateTimeKind.Utc
                ? bookingDateValue
                : DateTime.SpecifyKind(bookingDateValue, DateTimeKind.Utc);

            var eventDate = bookingDateUtc.Date;
            var today = DateTime.UtcNow.Date;

            if (eventDate < today.AddDays(30))
                throw new Exception("Booking must be made at least 30 days in advance");

            decimal basePrice = 0;
            decimal servicesPrice = 0;
            decimal totalPrice = 0;
            TimeSpan startTime;
            TimeSpan endTime;

            if (dto.TimeSlotId.HasValue)
            {
                var selectedSlot = venue.TimeSlots.FirstOrDefault(slot => slot.Id == dto.TimeSlotId.Value);

                if (selectedSlot == null)
                    throw new Exception("The selected time slot does not belong to this venue.");

                if (!selectedSlot.IsActive)
                    throw new Exception("The selected time slot is inactive.");

                startTime = selectedSlot.StartTime;
                endTime = selectedSlot.EndTime;
                basePrice = selectedSlot.Price;

                var existingBookings = await _bookingRepo.GetByVenueAndDate(dto.VenueId, bookingDateUtc);

                foreach (var bookingItem in existingBookings)
                {
                    if (startTime < bookingItem.EndTime && endTime > bookingItem.StartTime)
                        throw new Exception("Time not available");
                }
            }
            else if (venue.TimeSlots.Any(slot => slot.IsActive))
            {
                throw new Exception("Select one of the available venue time slots.");
            }
            else if (venue.PricingType == PricingType.Hourly)
            {
                if (!dto.StartTime.HasValue)
                    throw new Exception("Start time is required.");

                if (!dto.EndTime.HasValue)
                    throw new Exception("End time is required.");

                if (dto.EndTime.Value <= dto.StartTime.Value)
                    throw new Exception("End time must be after start time");

                if (!venue.PricePerHour.HasValue || venue.PricePerHour.Value <= 0)
                    throw new Exception("This venue does not have a valid hourly price");

                startTime = dto.StartTime.Value;
                endTime = dto.EndTime.Value;

                var existingBookings = await _bookingRepo.GetByVenueAndDate(dto.VenueId, bookingDateUtc);

                foreach (var bookingItem in existingBookings)
                {
                    if (startTime < bookingItem.EndTime && endTime > bookingItem.StartTime)
                        throw new Exception("Time not available");
                }

                var duration = dto.EndTime - dto.StartTime;
                var duration = endTime - startTime;

                if (duration.TotalHours < 1)
                    throw new Exception("Minimum booking duration is 1 hour.");

                basePrice = (decimal)duration.TotalHours * venue.PricePerHour.Value;
            }
            else if (venue.PricingType == PricingType.FixedSlots)
            {
                if (!dto.StartTime.HasValue)
                    throw new Exception("Start time is required.");

                if (!dto.EndTime.HasValue)
                    throw new Exception("End time is required.");

                startTime = dto.StartTime.Value;
                endTime = dto.EndTime.Value;

                var slotDate = DateOnly.FromDateTime(bookingDateUtc);

                var slot = await _venueAvailabilityRepo.GetSlotAsync(
                    dto.VenueId,
                    slotDate,
                    startTime,
                    endTime);

                if (slot == null)
                    throw new Exception("This slot does not exist");

                if (slot.IsBooked)
                    throw new Exception("This slot is already booked");

                var existingBooking = await _bookingRepo.GetByVenueAndDate(dto.VenueId, bookingDateUtc);

                if (existingBooking.Any(b =>
                    b.StartTime == dto.StartTime &&
                    b.EndTime == dto.EndTime))
                {
                    throw new Exception("This slot has just been booked.");
                }

                basePrice = slot.Price;
                slot.MarkAsBooked();


                basePrice = slot.Price;
                slot.MarkAsBooked();
            }
            else
            {
                throw new Exception("Invalid pricing type");
            }

            var selectedOptions = new List<VenueServiceOption>();

            if (dto.VenueServiceOptionIds != null && dto.VenueServiceOptionIds.Any())
            {
                selectedOptions = await _venueServiceOptionRepo.GetByIdsAsync(dto.VenueServiceOptionIds);

                if (selectedOptions.Count != dto.VenueServiceOptionIds.Count)
                    throw new Exception("One or more selected services are invalid.");

                if (selectedOptions.Any(x => x.VenueId != dto.VenueId))
                    throw new Exception("Selected services do not belong to this venue.");

                servicesPrice = selectedOptions.Sum(x => x.Price);
            }

            var totalPrice = basePrice + servicesPrice;

            var booking = new Booking(
                dto.VenueId,
                userId,
                bookingDateUtc,
                dto.StartTime,
                dto.EndTime,
                dto.GuestsCount,
                basePrice,
                servicesPrice,
                totalPrice
                startTime,
                endTime,
                dto.GuestsCount,
                basePrice,
                servicesPrice,
                totalPrice,
                dto.BrideIdDocumentDataUrl,
                dto.BridegroomIdDocumentDataUrl
            );

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            if (selectedOptions.Any())
            {
                var bookingServices = selectedOptions
                    .Select(x => new BookingSelectedService(booking.Id, x.Id, x.Price))
                    .ToList();

                await _bookingSelectedServiceRepo.AddRangeAsync(bookingServices);
                await _bookingRepo.SaveChangesAsync();
            }

            if (venue.PricingType == PricingType.FixedSlots && !dto.TimeSlotId.HasValue)
            {
                await _venueAvailabilityRepo.SaveChangesAsync();
            }

            var user = await _userRepo.GetUserByIdAsync(userId);

            await _emailService.SendEmailAsync(
                user.Email,
                "Booking Request Received",
                $@"
    <p>Dear {user.FirstName},</p>

    <p>Your booking request has been received successfully.</p>

    <p>We will review your request within <strong>24-48 hours</strong>.</p>

    <p>Best regards,<br/>Events Team</p>
    "
            );

            return new CreateBookingResponseDto
            {
                BookingId = booking.Id,
                VenueId = venue.Id,
                VenueName = venue.Name,
                Date = booking.BookingDate,
                Time = $"{booking.StartTime} - {booking.EndTime}",
                BasePrice = basePrice,
                ServicesPrice = servicesPrice,
                TotalPrice = totalPrice,
                Status = booking.Status.ToString(),
                Services = selectedOptions.Select(x => new SelectedServiceResponseDto
                {
                    VenueServiceOptionId = x.Id,
                    ServiceName = x.Service?.Name ?? string.Empty,
                    Price = x.Price
                }).ToList()
            };
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
                BasePrice = b.BasePrice,
                ServicesPrice = b.ServicesPrice,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                BrideIdDocumentDataUrl = b.BrideIdDocumentDataUrl,
                BridegroomIdDocumentDataUrl = b.BridegroomIdDocumentDataUrl,
                Services = MapSelectedServices(b)
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
                BasePrice = b.BasePrice,
                ServicesPrice = b.ServicesPrice,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                BrideIdDocumentDataUrl = b.BrideIdDocumentDataUrl,
                BridegroomIdDocumentDataUrl = b.BridegroomIdDocumentDataUrl,
                Services = MapSelectedServices(b)
            }).ToList();
        }

        public async Task Approve(int bookingId, int ownerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            if (booking.Venue.Company.UserId != ownerId)
                throw new Exception("Not allowed");

            if (booking.Status != BookingStatusEnum.Pending)
                throw new Exception("Only pending bookings can be approved.");

            if (booking.Payment == null || booking.Payment.Status != PaymentStatus.Paid)
                throw new Exception("Booking cannot be approved before payment.");

            booking.Approve(ownerId);
            await _bookingRepo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                booking.User.Email,
                "Your booking has been successfully confirmed 🎉",
                $@"
                <h2>Your booking has been approved ✅</h2>
                <p>Venue: {booking.Venue.Name}</p>
                <p>Date: {booking.BookingDate:yyyy-MM-dd}</p>
                <p>Time: {booking.StartTime} - {booking.EndTime}</p>
                ");
        }

        public async Task Reject(int bookingId, int ownerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            if (booking.Venue.Company.UserId != ownerId)
                throw new Exception("Not allowed");

            if (booking.Status != BookingStatusEnum.Pending)
                throw new Exception("Only pending bookings can be rejected.");

            booking.Reject(ownerId);
            await _bookingRepo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                booking.User.Email,
                "Unfortunately, your booking request has been rejected.",
                $@"
                <h2>Your booking has been rejected ❌</h2>
                <p>Venue: {booking.Venue.Name}</p>
                <p>Date: {booking.BookingDate:yyyy-MM-dd}</p>
                <p>Time: {booking.StartTime} - {booking.EndTime}</p>
                ");
        }

        public async Task<string> Cancel(int bookingId, int userId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            if (booking.UserId != userId)
                throw new Exception("Not allowed");

            var eventDate = booking.BookingDate.Date;
            var today = DateTime.UtcNow.Date;
            var daysLeft = (eventDate - today).TotalDays;

            booking.Cancel();

            if (booking.Venue.PricingType == PricingType.FixedSlots)
            {
                var slot = await _venueAvailabilityRepo.GetSlotAsync(
                    booking.VenueId,
                    DateOnly.FromDateTime(booking.BookingDate),
                    booking.StartTime,
                    booking.EndTime);

                if (slot != null)
                {
                    slot.MarkAsAvailable();
                    await _venueAvailabilityRepo.SaveChangesAsync();
                }
            }

            await _bookingRepo.SaveChangesAsync();

            if (daysLeft < 14)
                return "Booking cancelled, but deposit is non-refundable (less than 14 days left).";

            return "Booking cancelled successfully.";
        }
    }
}
