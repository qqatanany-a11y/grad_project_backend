using events.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.domain.DomainConfig
{
    public class PaymentConfig: IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.PaymentMethod)
                   .HasMaxLength(50);

            builder.Property(p => p.Status)
                   .HasConversion<string>() 
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(p => p.PaidAt)
                   .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                   .HasDefaultValueSql("now()");


        }

    }


}
