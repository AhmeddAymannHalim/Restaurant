using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(a => a.ArName).IsUnique();
        builder.HasIndex(a => a.EnName).IsUnique();

        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        // per base unit, so a gram of flour needs more than 2 decimals
        builder.Property(a => a.Cost).HasPrecision(18, 6);
        builder.Ignore(a => a.Stocks);
        builder.Ignore(a => a.Units);

        builder
            .HasOne(a => a.ProductCategory)
            .WithMany(a => a.Products)
            .HasForeignKey(a => a.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
