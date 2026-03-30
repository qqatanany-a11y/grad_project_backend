namespace events.domain.Entities;

public class Review
{

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid VenueId { get; private set; }
    public Venue Venue { get; private set; } = null!;

    public Guid BookingId { get; private set; }
    public Booking Booking { get; private set; } = null!;

    public int Rating { get; private set; }
    public string? Comment { get; private set; }
}