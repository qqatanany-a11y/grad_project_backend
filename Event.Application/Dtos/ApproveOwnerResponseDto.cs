namespace Event.Application.Dtos
{
    public class ApproveOwnerResponseDto
    {
        public string Message { get; set; }
        public int RequestId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
