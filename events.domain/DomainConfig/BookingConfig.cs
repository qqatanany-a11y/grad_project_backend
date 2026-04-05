using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class BookingConfig : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).ValueGeneratedOnAdd();
            builder.Property(b => b.BookingDate).IsRequired();
            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.TotalPrice).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(b => b.Status).IsRequired();
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.GuestsCount).IsRequired(false);

            builder.HasOne(b => b.Client)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.EventType)
                .WithMany()
                .HasForeignKey(b => b.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Review)
                .WithOne(r => r.Booking)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}