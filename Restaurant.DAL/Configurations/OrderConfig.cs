using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(a => a.OrderCode).IsRequired().HasMaxLength(30);
        builder.Property(a => a.CustomerName).HasMaxLength(100);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.PhoneNumber).HasMaxLength(11);
        builder.HasIndex(a => a.OrderCode).IsUnique();
        builder.HasIndex(a => a.OrderDate);
        builder.HasIndex(a => new { a.Cash, a.FinishShift });

        builder.HasOne(a => a.Seller).WithMany().HasForeignKey(a => a.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Receiver).WithMany().HasForeignKey(a => a.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Table).WithMany().HasForeignKey(a => a.TableId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Customer).WithMany().HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Employee).WithMany(a => a.Orders).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.DeliveryZone).WithMany().HasForeignKey(a => a.DeliveryZoneId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.DeliveryFee).HasPrecision(18, 2);
    }
}

public class OrderMenuItemConfig : IEntityTypeConfiguration<OrderMenuItem>
{
    public void Configure(EntityTypeBuilder<OrderMenuItem> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);

        builder
            .HasOne(a => a.Order)
            .WithMany(a => a.OrderMenuItems)
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.MenuItem)
            .WithMany(a => a.OrderMenuItems)
            .HasForeignKey(a => a.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderIngredientConfig : IEntityTypeConfiguration<OrderIngredient>
{
    public void Configure(EntityTypeBuilder<OrderIngredient> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);

        builder
            .HasOne(a => a.OrderMenuItem)
            .WithMany(a => a.Ingredients)
            .HasForeignKey(a => a.OrderMenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Store>()
            .WithMany()
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
