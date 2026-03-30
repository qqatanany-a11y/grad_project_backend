using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class EventTypeConfig : IEntityTypeConfiguration<EventType>
    {
        public void Configure(EntityTypeBuilder<EventType> builder)
        {
        
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(e => e.VenueEventTypes)
                   .WithOne(vet => vet.EventType)
                   .HasForeignKey(vet => vet.EventTypeId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
