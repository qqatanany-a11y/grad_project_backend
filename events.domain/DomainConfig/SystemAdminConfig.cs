using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using events.domain.Entities;

namespace events.domain.DomainConfig
{
    public class SystemAdminConfig : IEntityTypeConfiguration<SystemAdmin>
    {
        public void Configure(EntityTypeBuilder<SystemAdmin> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.MiddleName)
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Ignore(x => x.FullName);
        }
    }
}