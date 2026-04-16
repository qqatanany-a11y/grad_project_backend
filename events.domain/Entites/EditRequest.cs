using events.domain.Entites;

namespace events.domain.Entities
{
    public class EditRequest : BaseEntity
    {
        private EditRequest() { }

        public int OwnerId { get; private set; }
        public User Owner { get; private set; }

        public EditRequestTypeEnum Type { get; private set; }
        public EditRequestStatusEnum Status { get; private set; } = EditRequestStatusEnum.Pending;

        public int? TargetId { get; private set; } // VenueId إذا كان التعديل على القاعة
        public string RequestedDataJson { get; private set; }

        public int? ReviewedByAdminId { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public string? RejectionReason { get; private set; }

        public EditRequest(
            int ownerId,
            EditRequestTypeEnum type,
            int? targetId,
            string requestedDataJson)
        {
            OwnerId = ownerId;
            Type = type;
            TargetId = targetId;
            RequestedDataJson = requestedDataJson;
            Status = EditRequestStatusEnum.Pending;
        }

        public void Approve(int adminId)
        {
            Status = EditRequestStatusEnum.Approved;
            ReviewedByAdminId = adminId;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(int adminId, string? reason)
        {
            Status = EditRequestStatusEnum.Rejected;
            ReviewedByAdminId = adminId;
            ReviewedAt = DateTime.UtcNow;
            RejectionReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}