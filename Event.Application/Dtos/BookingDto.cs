namespace Event.Application.Dtos
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string VenueName { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}