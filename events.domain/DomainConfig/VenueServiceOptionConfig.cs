using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class VenueServiceOptionConfig : IEntityTypeConfiguration<VenueServiceOption>
    {
        public void Configure(EntityTypeBuilder<VenueServiceOption> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            builder.HasOne(x => x.Venue)
                   .WithMany(x => x.VenueServices)
                   .HasForeignKey(x => x.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Service)
                   .WithMany(x => x.VenueServices)
                   .HasForeignKey(x => x.ServiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.VenueId, x.ServiceId }).IsUnique();
        }
    }
}