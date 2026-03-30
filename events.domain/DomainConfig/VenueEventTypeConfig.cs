using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class VenueEventTypeConfig : IEntityTypeConfiguration<VenueEventType>
    {
        public void Configure(EntityTypeBuilder<VenueEventType> builder)
        {
            

            builder.HasKey(vet => new { vet.VenueId, vet.EventTypeId });

        
            builder.HasOne(vet => vet.Venue)
                   .WithMany(v => v.VenueEventTypes)
                   .HasForeignKey(vet => vet.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(vet => vet.EventType)
                   .WithMany(e => e.VenueEventTypes)
                   .HasForeignKey(vet => vet.EventTypeId)
                   .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
