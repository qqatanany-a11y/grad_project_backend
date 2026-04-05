using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class OwnerProfileConfig : IEntityTypeConfiguration<OwnerProfile>
    {
        public void Configure(EntityTypeBuilder<OwnerProfile> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.CompanyName).HasMaxLength(200).IsRequired();
            builder.Property(p => p.BusinessPhone).HasMaxLength(100).IsRequired();
            builder.Property(p => p.BusinessAddress).HasMaxLength(500).IsRequired();

            builder.HasOne(p => p.User)
                   .WithOne()
                   .HasForeignKey<OwnerProfile>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}