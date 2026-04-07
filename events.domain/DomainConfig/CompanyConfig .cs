using events.domain.Entites;
using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class CompanyConfig : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
            builder.Property(c => c.Location).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(200).IsRequired();

            // ← جديد: شركة وحدة لكل Owner
            builder.HasOne(c => c.User)
                   .WithOne(u => u.Company)
                   .HasForeignKey<Company>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ← جديد: شركة عندها كتير قاعات
            builder.HasMany(c => c.Venues)
                   .WithOne(v => v.Company)
                   .HasForeignKey(v => v.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}