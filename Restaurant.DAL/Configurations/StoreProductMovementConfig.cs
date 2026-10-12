using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class StoreProductMovementConfig : IEntityTypeConfiguration<StoreProductMovement>
{
    public void Configure(EntityTypeBuilder<StoreProductMovement> builder)
    {
        builder.Property(a => a.QuantityDelta).HasPrecision(18, 3);
        builder.Property(a => a.ReferenceCode).HasMaxLength(20);

        builder.HasIndex(a => new { a.StoreId, a.ProductId });
        builder.HasIndex(a => a.MovementDate);

        builder
            .HasOne(a => a.Store)
            .WithMany()
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
