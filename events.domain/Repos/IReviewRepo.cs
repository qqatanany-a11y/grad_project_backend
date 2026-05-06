using events.domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace events.domain.Repos
{
    public interface IReviewRepo
    {
        Task AddAsync(Review review);
        Task<bool> ExistsForBooking(int bookingId);
        Task<List<Review>> GetByVenueIdAsync(int venueId);
    }
}
