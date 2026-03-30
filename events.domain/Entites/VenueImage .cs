namespace events.domain.Entities
{
    public class VenueImage
    {
        public int Id { get; set; }
        public int VenueId { get; set; }         
        public string ImageUrl { get; set; } = null!;
        public bool IsCover { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      
        public virtual Venue Venue { get; set; } = null!;
    }
}
