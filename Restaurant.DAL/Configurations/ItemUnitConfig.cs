using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ItemUnitConfig : IEntityTypeConfiguration<ItemUnit>
{
    public void Configure(EntityTypeBuilder<ItemUnit> builder)
    {
        builder.Property(a => a.Whole).HasPrecision(18, 6);
        builder.Property(a => a.HalfWhole).HasPrecision(18, 6);
        builder.Property(a => a.Retail).HasPrecision(18, 6);
        builder.Property(a => a.EndUsers).HasPrecision(18, 6);
        builder.Property(a => a.WeightedAverage).HasPrecision(18, 6);
        builder.Property(a => a.Export).HasPrecision(18, 6);
        builder.Property(a => a.LastPurchase).HasPrecision(18, 6);
    }
}
