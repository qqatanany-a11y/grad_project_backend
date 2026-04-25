using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class ServiceConfig : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.Description)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.HasMany(x => x.VenueServices)
                   .WithOne(x => x.Service)
                   .HasForeignKey(x => x.ServiceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}