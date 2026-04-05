using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class VenueAvailabilityConfig : IEntityTypeConfiguration<VenueAvailability>
    {
        public void Configure(EntityTypeBuilder<VenueAvailability> builder)
        {

            builder.HasKey(v => v.Id);

            builder.Property(v => v.StartTime)
                   .IsRequired();

            builder.Property(v => v.EndTime)
                   .IsRequired(false);

            builder.Property(v => v.IsAvailable)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(v => v.CreatedAt)
                   .HasDefaultValueSql("now()");

            builder.HasOne(v => v.Venue)
                   .WithMany(vn => vn.Availabilities) 
                   .HasForeignKey(v => v.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

         
        }
    }
}
