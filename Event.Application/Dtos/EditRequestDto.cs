namespace Event.Application.Dtos
{
    public class EditRequestDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int? TargetId { get; set; }
        public string RequestedDataJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}