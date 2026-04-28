using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class VenueTimeSlotConfig : IEntityTypeConfiguration<VenueTimeSlot>
    {
        public void Configure(EntityTypeBuilder<VenueTimeSlot> builder)
        {
            builder.HasKey(slot => slot.Id);

            builder.Property(slot => slot.StartTime)
                .IsRequired();

            builder.Property(slot => slot.EndTime)
                .IsRequired();

            builder.Property(slot => slot.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(slot => slot.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.HasIndex(slot => new { slot.VenueId, slot.StartTime, slot.EndTime })
                .IsUnique();
        }
    }
}
