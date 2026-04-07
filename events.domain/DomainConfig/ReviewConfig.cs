using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class ReviewConfig : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment).HasMaxLength(500).IsRequired(false);



            builder.HasOne(r => r.Venue)
                   .WithMany(v => v.Reviews)
                   .HasForeignKey(r => r.VenueId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Booking)
                   .WithOne(b => b.Review)
                   .HasForeignKey<Review>(r => r.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}