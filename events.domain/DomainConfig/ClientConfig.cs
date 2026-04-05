using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class ClientConfig : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.MiddleName).HasMaxLength(100).IsRequired(false);
            builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(200).IsRequired();
            builder.Property(c => c.PasswordHash).HasMaxLength(500).IsRequired();
            builder.Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(c => c.IsActive).HasDefaultValue(true).IsRequired();

            builder.HasIndex(c => c.Email).IsUnique();
        }
    }
}