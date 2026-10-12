using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class MenuItemComponentConfig : IEntityTypeConfiguration<MenuItemComponent>
{
    public void Configure(EntityTypeBuilder<MenuItemComponent> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);

        builder
            .HasOne(a => a.MenuItem)
            .WithMany(a => a.Components)
            .HasForeignKey(a => a.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Product)
            .WithMany(a => a.MenuItemComponents)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
