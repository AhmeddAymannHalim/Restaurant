using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class OnlineOrderConfig : IEntityTypeConfiguration<OnlineOrder>
{
    public void Configure(EntityTypeBuilder<OnlineOrder> builder)
    {
        builder.Property(a => a.CustomerName).HasMaxLength(100);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.PhoneNumber).HasMaxLength(11);
        builder.Property(a => a.MobileNumber).HasMaxLength(11);
        builder.Property(a => a.Status).HasDefaultValue(Shared.Enums.OnlineOrderStatus.Pending);
        builder.Property(a => a.RejectionReason).HasMaxLength(250);

        builder
            .HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Table)
            .WithMany()
            .HasForeignKey(a => a.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.DeliveryZone)
            .WithMany()
            .HasForeignKey(a => a.DeliveryZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.DeliveryFee).HasPrecision(18, 2);
        builder.Property(a => a.Tax).HasPrecision(18, 2);
        builder.Property(a => a.Service).HasPrecision(18, 2);
    }
}

public class OnlineOrderMenuItemConfig : IEntityTypeConfiguration<OnlineOrderMenuItem>
{
    public void Configure(EntityTypeBuilder<OnlineOrderMenuItem> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);

        builder
            .HasOne(a => a.OnlineOrder)
            .WithMany(a => a.OnlineOrderMenuItems)
            .HasForeignKey(a => a.OnlineOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.MenuItem)
            .WithMany(a => a.OnlineOrderMenuItems)
            .HasForeignKey(a => a.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
