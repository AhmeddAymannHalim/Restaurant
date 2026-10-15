using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

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
