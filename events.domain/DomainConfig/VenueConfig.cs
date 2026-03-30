using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class VenueConfig : IEntityTypeConfiguration<Venue>
    {
        public void Configure(EntityTypeBuilder<Venue> builder)
        {


            builder.HasKey(v => v.Id);

            builder.Property(v => v.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(v => v.Description)
                   .HasMaxLength(1000);

            builder.Property(v => v.City)
                   .HasMaxLength(100);

            builder.Property(v => v.Address)
                   .HasMaxLength(250);

            builder.Property(v => v.Capacity)
                   .IsRequired();

            builder.Property(v => v.MinimalPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(v => v.IsActive)
                   .HasDefaultValue(true);

           
            builder.HasOne(v => v.Owner)
                   .WithMany(u => u.Venues) 
                   .HasForeignKey(v => v.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(v => v.Images)
                   .WithOne(i => i.Venue)
                   .HasForeignKey(i => i.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(v => v.VenueEventTypes)
                   .WithOne(vet => vet.Venue)
                   .HasForeignKey(vet => vet.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(v => v.Availabilities)
                   .WithOne(a => a.Venue)
                   .HasForeignKey(a => a.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(v => v.Bookings)
                   .WithOne(b => b.Venue)
                   .HasForeignKey(b => b.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(v => v.Reviews)
                   .WithOne(r => r.Venue)
                   .HasForeignKey(r => r.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
