using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class BookingSelectedServiceConfig : IEntityTypeConfiguration<BookingSelectedService>
    {
        public void Configure(EntityTypeBuilder<BookingSelectedService> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.HasOne(x => x.Booking)
                   .WithMany(x => x.SelectedServices)
                   .HasForeignKey(x => x.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.VenueServiceOption)
                   .WithMany()
                   .HasForeignKey(x => x.VenueServiceOptionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}