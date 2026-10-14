using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class StoreItemUnitConfig : IEntityTypeConfiguration<StoreItemUnit>
{
    public void Configure(EntityTypeBuilder<StoreItemUnit> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);
    }
}
