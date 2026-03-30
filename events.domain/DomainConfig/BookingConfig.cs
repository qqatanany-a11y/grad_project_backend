using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class BookingConfig : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            // =========================
            // Primary Key
            // =========================
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .ValueGeneratedOnAdd();

            // =========================
            // Required Fields
            // =========================
            builder.Property(b => b.BookingDate)
                .IsRequired();

            builder.Property(b => b.StartTime)
                .IsRequired();

            builder.Property(b => b.EndTime)
                .IsRequired();

            builder.Property(b => b.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.Status)
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            // =========================
            // Optional Fields
            // =========================
            builder.Property(b => b.GuestsCount)
                .IsRequired(false);

            // =========================
            // Relations
            // =========================

            // Booking → User (optional)
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Booking → Venue (required)
            builder.HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking → EventType (optional)
            builder.HasOne(b => b.EventType)
                .WithMany()
                .HasForeignKey(b => b.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Booking → Payment (1 - 1)
            builder.HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking → Review (1 - 1)
            builder.HasOne(b => b.Review)
                .WithOne(r => r.Booking)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}