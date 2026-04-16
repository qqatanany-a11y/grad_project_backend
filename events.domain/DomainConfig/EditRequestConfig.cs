using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class EditRequestConfig : IEntityTypeConfiguration<EditRequest>
    {
        public void Configure(EntityTypeBuilder<EditRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestedDataJson)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion<string>()   //  Profile / Venue
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()   //  Pending / Approved / Rejected
                .IsRequired();

            builder.Property(x => x.RejectionReason)
                .IsRequired(false);

            builder.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}