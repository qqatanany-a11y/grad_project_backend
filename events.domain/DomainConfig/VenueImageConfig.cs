using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class VenueImageConfig : IEntityTypeConfiguration<VenueImage>
    {
        public void Configure(EntityTypeBuilder<VenueImage> builder)
        {
            

            builder.HasKey(v => v.Id);

            builder.Property(v => v.ImageUrl)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(v => v.IsCover)
                   .HasDefaultValue(false);

            builder.HasOne(v => v.Venue)
                   .WithMany(vn => vn.Images)
                   .HasForeignKey(v => v.VenueId)
                   .OnDelete(DeleteBehavior.Cascade);

          
        }
    }
}
