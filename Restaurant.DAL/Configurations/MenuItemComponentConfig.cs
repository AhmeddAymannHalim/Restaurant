using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class MenuItemComponentConfig : IEntityTypeConfiguration<MenuItemComponent>
{
    public void Configure(EntityTypeBuilder<MenuItemComponent> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);
        builder.Property(a => a.Cost).HasPrecision(18, 2);

        // Store -> MenuItem -> MenuItemComponent and Store -> StoreProduct -> MenuItemComponent would be two cascade paths.
        builder
            .HasOne(a => a.StoreProduct)
            .WithMany()
            .HasForeignKey(a => a.StoreProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
