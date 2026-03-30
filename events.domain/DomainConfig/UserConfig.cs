using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.HasKey(p => p.Id);

            builder.Property(p => p.FullName).HasMaxLength(200).IsRequired(true);
            builder.Property(p => p.Email).HasMaxLength(200).IsRequired(true);
            builder.Property(p => p.PasswordHash).HasMaxLength(20).IsRequired(true);
            builder.Property(p => p.PhoneNumber).HasMaxLength(20).IsRequired(true);
            builder.Property(p => p.IsActive).HasDefaultValue(true).IsRequired(true);
            builder.Property(p => p.CreatedAt).IsRequired(true);

            builder.Property(p => p.RoleId).IsRequired(true);
            builder.HasOne(p => p.Role).WithMany().HasForeignKey(p => p.RoleId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
