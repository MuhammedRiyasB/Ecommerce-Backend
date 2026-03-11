using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.OrderId);
            builder.Property(o => o.TransactionId).IsRequired().HasMaxLength(100);
            builder.HasIndex(o => o.TransactionId).IsUnique();
            builder.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
            builder.HasOne(o => o.Address).WithMany(a => a.Orders).HasForeignKey(o => o.AddressId);
        }
    }
}
