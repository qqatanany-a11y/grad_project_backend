using Event.Application.Dtos;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class ReviewService
    {
        private readonly IReviewRepo _reviewRepo;
        private readonly IBookingRepo _bookingRepo;

        public ReviewService(IReviewRepo reviewRepo, IBookingRepo bookingRepo)
        {
            _reviewRepo = reviewRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<string> AddReview(int userId, AddReviewDto dto)
        {
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            // لازم يكون المستخدم صاحب الحجز
            if (booking.UserId != userId)
                throw new Exception("Not allowed");

            // لازم يكون الحجز Confirmed
            if (booking.Status != BookingStatusEnum.Confirmed)
                throw new Exception("You can only review confirmed bookings");

            // ما يسمح أكثر من review
            var exists = await _reviewRepo.ExistsForBooking(dto.BookingId);
            if (exists)
                throw new Exception("Review already exists");

            var review = new Review(
                booking.VenueId,
                dto.BookingId,
                userId,
                dto.Rating,
                dto.Comment
            );

            await _reviewRepo.AddAsync(review);

            return "Review added successfully";
        }
    }
}