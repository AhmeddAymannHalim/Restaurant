using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ItemUnitPriceConfig : IEntityTypeConfiguration<ItemUnitPrice>
{
    public void Configure(EntityTypeBuilder<ItemUnitPrice> builder)
    {
        // a price of a unit can be a fraction of a cent (one gram), so keep 6 decimals
        builder.Property(a => a.Price).HasPrecision(18, 6);
        builder.HasIndex(a => new { a.ItemUnitId, a.SellTypeId }).IsUnique();

        builder
            .HasOne(a => a.ItemUnit)
            .WithMany()
            .HasForeignKey(a => a.ItemUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.SellType)
            .WithMany()
            .HasForeignKey(a => a.SellTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
