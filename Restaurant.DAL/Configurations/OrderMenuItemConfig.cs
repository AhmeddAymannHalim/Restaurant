using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class OrderMenuItemConfig : IEntityTypeConfiguration<OrderMenuItem>
{
    public void Configure(EntityTypeBuilder<OrderMenuItem> builder)
    {
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.HasOne(a => a.BillDetail).WithOne().HasForeignKey<OrderMenuItem>(a => a.Id).OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Order)
            .WithMany(a => a.Lines)
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(a => a.MenuItem)
            .WithMany()
            .HasForeignKey(a => a.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
