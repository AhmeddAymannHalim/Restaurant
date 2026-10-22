using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.OrderCode).IsRequired().HasMaxLength(30);
        builder.Property(a => a.CustomerName).HasMaxLength(100);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.PhoneNumber).HasMaxLength(11);
        builder.Property(a => a.Service).HasPrecision(18, 2);
        builder.Property(a => a.Tax).HasPrecision(18, 2);
        builder.Property(a => a.DeliveryFee).HasPrecision(18, 2);
        builder.HasIndex(a => a.OrderCode).IsUnique();
        builder.HasIndex(a => a.Cash);

        builder.HasOne(a => a.Bill).WithOne().HasForeignKey<Order>(a => a.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Seller).WithMany().HasForeignKey(a => a.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Receiver).WithMany().HasForeignKey(a => a.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Table).WithMany().HasForeignKey(a => a.TableId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Customer).WithMany().HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.DeliveryZone).WithMany().HasForeignKey(a => a.DeliveryZoneId).OnDelete(DeleteBehavior.Restrict);
    }
}
