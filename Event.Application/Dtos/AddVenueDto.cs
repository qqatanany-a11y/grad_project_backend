namespace Event.Application.Dtos
{
    public class AddVenueDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CopmanyId { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
        public decimal MinimalPrice { get; set; }
    }
}