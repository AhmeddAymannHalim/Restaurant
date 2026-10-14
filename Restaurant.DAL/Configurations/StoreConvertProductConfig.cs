using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class StoreConvertProductConfig : IEntityTypeConfiguration<StoreConvertProduct>
{
    public void Configure(EntityTypeBuilder<StoreConvertProduct> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);

        builder
            .HasOne(a => a.StoreConvert)
            .WithMany(a => a.StoreConvertProducts)
            .HasForeignKey(a => a.StoreConvertId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
