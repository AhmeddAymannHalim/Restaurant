using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class StoreProductConfig : IEntityTypeConfiguration<StoreProduct>
{
    public void Configure(EntityTypeBuilder<StoreProduct> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);

        builder.HasIndex(a => new { a.StoreId, a.ProductId }).IsUnique();
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder
            .HasOne(a => a.Store)
            .WithMany(a => a.StoreProducts)
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Product)
            .WithMany(a => a.StoreProducts)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
