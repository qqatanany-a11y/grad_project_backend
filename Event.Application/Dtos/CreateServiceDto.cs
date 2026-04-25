namespace Event.Application.Dtos
{
    public class CreateServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}