using Event.Application.Dtos;
using Event.Application.Helpers;
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
        private readonly IMediaStorageService _mediaStorageService;

        public BookingService(
            IBookingRepo bookingRepo,
            IVenueRepo venueRepo,
            IUserRepo userRepo,
            IVenueAvailabilityRepo venueAvailabilityRepo,
            IVenueServiceOptionRepo venueServiceOptionRepo,
            IBookingSelectedServiceRepo bookingSelectedServiceRepo,
            IEmailService emailService,
            IMediaStorageService mediaStorageService)
        {
            _bookingRepo = bookingRepo;
            _venueRepo = venueRepo;
            _userRepo = userRepo;
            _venueAvailabilityRepo = venueAvailabilityRepo;
            _venueServiceOptionRepo = venueServiceOptionRepo;
            _bookingSelectedServiceRepo = bookingSelectedServiceRepo;
            _emailService = emailService;
            _mediaStorageService = mediaStorageService;
        }

        public async Task<CreateBookingResponseDto> CreateBooking(int userId, CreateBookingDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(dto.VenueId);
            if (venue == null)
            {
                throw new Exception("Venue not found");
            }

            if (!venue.IsActive)
            {
                throw new Exception("This venue is currently inactive.");
            }

            var bookingDateSource = dto.BookingDate ?? dto.Date;
            if (!bookingDateSource.HasValue)
            {
                throw new Exception("Booking date is required.");
            }

            if (dto.GuestsCount.HasValue && dto.GuestsCount.Value <= 0)
            {
                throw new Exception("Guests count must be greater than zero.");
            }

            if (dto.GuestsCount.HasValue && dto.GuestsCount.Value > venue.Capacity)
            {
                throw new Exception("Guests count exceeds venue capacity.");
            }

            var bookingDateValue = bookingDateSource.Value;
            var bookingDateUtc = bookingDateValue.Kind == DateTimeKind.Utc
                ? bookingDateValue
                : DateTime.SpecifyKind(bookingDateValue, DateTimeKind.Utc);

            if (bookingDateUtc.Date < DateTime.UtcNow.Date.AddDays(30))
            {
                throw new Exception("Booking must be made at least 30 days in advance");
            }

            var existingBookings = await _bookingRepo.GetByVenueAndDate(dto.VenueId, bookingDateUtc);

            decimal basePrice = 0;
            decimal servicesPrice = 0;
            TimeSpan startTime = default;
            TimeSpan endTime = default;
            VenueAvailability? selectedAvailabilitySlot = null;

            if (dto.VenueAvailabilityId.HasValue)
            {
                selectedAvailabilitySlot = await _venueAvailabilityRepo.GetByIdAsync(dto.VenueAvailabilityId.Value);

                if (selectedAvailabilitySlot == null)
                {
                    throw new Exception("The selected venue availability was not found.");
                }

                if (selectedAvailabilitySlot.VenueId != dto.VenueId)
                {
                    throw new Exception("The selected venue availability does not belong to this venue.");
                }

                if (selectedAvailabilitySlot.Date != DateOnly.FromDateTime(bookingDateUtc))
                {
                    throw new Exception("The selected venue availability does not match the booking date.");
                }
            }
            else if (dto.TimeSlotId.HasValue)
            {
                var selectedSlot = venue.TimeSlots.FirstOrDefault(slot => slot.Id == dto.TimeSlotId.Value);
                if (selectedSlot == null)
                {
                    throw new Exception("The selected time slot does not belong to this venue.");
                }

                if (!selectedSlot.IsActive)
                {
                    throw new Exception("The selected time slot is inactive.");
                }

                startTime = selectedSlot.StartTime;
                endTime = selectedSlot.EndTime;
                basePrice = selectedSlot.Price;
                EnsureNoOverlap(existingBookings, startTime, endTime);
            }
            else if (dto.StartTime.HasValue && dto.EndTime.HasValue)
            {
                selectedAvailabilitySlot = await _venueAvailabilityRepo.GetSlotAsync(
                    dto.VenueId,
                    DateOnly.FromDateTime(bookingDateUtc),
                    dto.StartTime.Value,
                    dto.EndTime.Value);

                if (selectedAvailabilitySlot == null)
                {
                    throw new Exception("Select one of the available venue slots.");
                }
            }
            else
            {
                throw new Exception("Select one of the available venue slots.");
            }

            if (selectedAvailabilitySlot != null)
            {
                if (selectedAvailabilitySlot.IsBooked)
                {
                    throw new Exception("This slot is already booked.");
                }

                startTime = selectedAvailabilitySlot.StartTime;
                endTime = selectedAvailabilitySlot.EndTime;
                EnsureNoOverlap(existingBookings, startTime, endTime);
                basePrice = selectedAvailabilitySlot.Price;
                selectedAvailabilitySlot.MarkAsBooked();
            }

            var selectedOptions = new List<VenueServiceOption>();
            if (dto.VenueServiceOptionIds.Count > 0)
            {
                selectedOptions = await _venueServiceOptionRepo.GetByIdsAsync(dto.VenueServiceOptionIds);

                if (selectedOptions.Count != dto.VenueServiceOptionIds.Count)
                {
                    throw new Exception("One or more selected services are invalid.");
                }

                if (selectedOptions.Any(option => option.VenueId != dto.VenueId))
                {
                    throw new Exception("Selected services do not belong to this venue.");
                }

                servicesPrice = selectedOptions.Sum(option => option.Price);
            }

            var totalPrice = basePrice + servicesPrice;

            var brideIdDocumentPath = await _mediaStorageService.NormalizeImageReferenceAsync(
                dto.BrideIdDocumentDataUrl,
                "bookings");
            var bridegroomIdDocumentPath = await _mediaStorageService.NormalizeImageReferenceAsync(
                dto.BridegroomIdDocumentDataUrl,
                "bookings");

            var booking = new Booking(
                dto.VenueId,
                userId,
                bookingDateUtc,
                startTime,
                endTime,
                dto.GuestsCount,
                basePrice,
                servicesPrice,
                totalPrice,
                brideIdDocumentPath,
                bridegroomIdDocumentPath);

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            if (selectedOptions.Count > 0)
            {
                var bookingServices = selectedOptions
                    .Select(option => new BookingSelectedService(booking.Id, option.Id, option.Price))
                    .ToList();

                await _bookingSelectedServiceRepo.AddRangeAsync(bookingServices);
            }

            if (selectedAvailabilitySlot != null)
            {
                await _venueAvailabilityRepo.SaveChangesAsync();
            }

            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Booking Request Received",
                    $@"
<p>Dear {user.FirstName},</p>
<p>Your booking request has been received successfully.</p>
<p>We will review your request within <strong>24-48 hours</strong>.</p>
<p>Best regards,<br/>Events Team</p>");
            }

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
                Services = selectedOptions.Select(option => new SelectedServiceResponseDto
                {
                    VenueServiceOptionId = option.Id,
                    ServiceName = option.Service?.Name ?? string.Empty,
                    Price = option.Price
                }).ToList()
            };
        }

        public async Task<List<BookingDto>> GetMyBookings(int userId)
        {
            var bookings = await _bookingRepo.GetUserBookings(userId);
            return bookings.Select(MapBooking).ToList();
        }

        public async Task<List<BookingDto>> GetOwnerBookings(int ownerId)
        {
            var bookings = await _bookingRepo.GetOwnerBookings(ownerId);
            return bookings.Select(MapBooking).ToList();
        }

        public async Task Approve(int bookingId, int ownerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }

            if (booking.Venue.Company.UserId != ownerId)
            {
                throw new Exception("Not allowed");
            }

            if (booking.Status != BookingStatusEnum.Pending)
            {
                throw new Exception("Only pending bookings can be approved.");
            }

            booking.Approve(ownerId);
            await _bookingRepo.SaveChangesAsync();

            var depositAmount = CalculateDepositAmount(booking.TotalPrice, booking.Venue.DepositPercentage);

            await _emailService.SendEmailAsync(
                booking.User.Email,
                "Your booking has been approved",
                $@"
<h2>Your booking has been approved</h2>
<p>Venue: {booking.Venue.Name}</p>
<p>Date: {booking.BookingDate:yyyy-MM-dd}</p>
<p>Time: {booking.StartTime} - {booking.EndTime}</p>
<p>Deposit amount: {depositAmount:0.00} JOD ({booking.Venue.DepositPercentage:0.##}%)</p>
<p>You can now complete the payment from your My Bookings page.</p>");
        }

        public async Task Reject(int bookingId, int ownerId, string? reason)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }

            if (booking.Venue.Company.UserId != ownerId)
            {
                throw new Exception("Not allowed");
            }

            if (booking.Status != BookingStatusEnum.Pending)
            {
                throw new Exception("Only pending bookings can be rejected.");
            }

            var normalizedReason = RejectReasonHelper.Normalize(reason);

            booking.Reject(ownerId);
            await _bookingRepo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                booking.User.Email,
                "Your booking has been rejected",
                $@"
<h2>Your booking has been rejected</h2>
<p>Venue: {booking.Venue.Name}</p>
<p>Date: {booking.BookingDate:yyyy-MM-dd}</p>
<p>Time: {booking.StartTime} - {booking.EndTime}</p>
<p>Reason: {normalizedReason}</p>");
        }

        public async Task<string> Cancel(int bookingId, int userId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }

            if (booking.UserId != userId)
            {
                throw new Exception("Not allowed");
            }

            if (booking.Status == BookingStatusEnum.Cancelled)
            {
                throw new Exception("This booking is already cancelled.");
            }

            if (booking.Status == BookingStatusEnum.Rejected)
            {
                throw new Exception("Rejected bookings cannot be cancelled.");
            }

            if (booking.Status != BookingStatusEnum.Pending && booking.Status != BookingStatusEnum.Confirmed)
            {
                throw new Exception("Only pending or confirmed bookings can be cancelled.");
            }

            var daysLeft = (booking.BookingDate.Date - DateTime.UtcNow.Date).TotalDays;

            booking.Cancel();

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

            await _bookingRepo.SaveChangesAsync();

            return daysLeft < 14
                ? "Booking cancelled, but deposit is non-refundable (less than 14 days left)."
                : "Booking cancelled successfully.";
        }

        private static void EnsureNoOverlap(IEnumerable<Booking> bookings, TimeSpan startTime, TimeSpan endTime)
        {
            if (bookings.Any(booking => startTime < booking.EndTime && endTime > booking.StartTime))
            {
                throw new Exception("Time not available");
            }
        }

        private BookingDto MapBooking(Booking booking)
        {
            var depositAmount = CalculateDepositAmount(booking.TotalPrice, booking.Venue.DepositPercentage);

            return new BookingDto
            {
                Id = booking.Id,
                VenueName = booking.Venue.Name,
                Date = booking.BookingDate,
                Time = $"{booking.StartTime} - {booking.EndTime}",
                BasePrice = booking.BasePrice,
                ServicesPrice = booking.ServicesPrice,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                DepositPercentage = booking.Venue.DepositPercentage,
                DepositAmount = depositAmount,
                CanPay = booking.Status == BookingStatusEnum.Confirmed &&
                         booking.Payment == null &&
                         depositAmount > 0,
                Payment = MapPayment(booking.Payment),
                BrideIdDocumentDataUrl = _mediaStorageService.SanitizePublicReference(booking.BrideIdDocumentDataUrl),
                BridegroomIdDocumentDataUrl = _mediaStorageService.SanitizePublicReference(booking.BridegroomIdDocumentDataUrl),
                Services = booking.SelectedServices.Select(service => new SelectedServiceResponseDto
                {
                    VenueServiceOptionId = service.VenueServiceOptionId,
                    ServiceName = service.VenueServiceOption?.Service?.Name ?? string.Empty,
                    Price = service.Price
                }).ToList()
            };
        }

        private BookingPaymentDto? MapPayment(Payment? payment)
        {
            if (payment == null)
            {
                return null;
            }

            return new BookingPaymentDto
            {
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                PaidAt = payment.PaidAt,
                CliqTransferImageDataUrl = _mediaStorageService.SanitizePublicReference(payment.CliqTransferImageDataUrl)
            };
        }

        private static decimal CalculateDepositAmount(decimal totalPrice, decimal depositPercentage)
        {
            if (depositPercentage <= 0)
            {
                return 0;
            }

            return decimal.Round(totalPrice * (depositPercentage / 100m), 2, MidpointRounding.AwayFromZero);
        }
    }
}
