
using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(p => p.MiddleName).HasMaxLength(100).IsRequired(false);
            builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();

            builder.Ignore(p => p.FullName);

            builder.Property(p => p.Email).HasMaxLength(100).IsRequired(true);
            builder.Property(p => p.PasswordHash).HasMaxLength(500).IsRequired(true);
            builder.Property(p => p.PhoneNumber).HasMaxLength(20).IsRequired(true);
            builder.Property(p => p.IsActive).HasDefaultValue(true).IsRequired(true);
            builder.Property(p => p.CreatedAt).IsRequired(true);

            builder.Property(p => p.RoleId).IsRequired(true);
            builder.HasOne(p => p.Role).WithMany().HasForeignKey(p => p.RoleId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}