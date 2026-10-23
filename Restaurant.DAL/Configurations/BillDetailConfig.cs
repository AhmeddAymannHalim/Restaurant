using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class BillDetailConfig : IEntityTypeConfiguration<BillDetail>
{
    public void Configure(EntityTypeBuilder<BillDetail> builder)
    {
        // a rate such as 0.001 (gram in a kilogram item) must not be rounded to 0.00
        builder.Property(a => a.TransRate).HasPrecision(18, 6);

        // 0.125 kilogram, or a price of 0.012 for one gram, must come back exactly when the invoice is edited, posted or deleted
        builder.Property(a => a.Quantity).HasPrecision(18, 6);
        builder.Property(a => a.UnitPrice).HasPrecision(18, 6);

        builder.Property(a => a.OwnDiscountPercentage).HasPrecision(18, 6);
        builder.Property(a => a.OwnDiscountValue).HasPrecision(18, 6);
        builder.Property(a => a.UnitCostAtPosting).HasPrecision(18, 6);
        builder.Property(a => a.DiscountFromBillLevel).HasPrecision(18, 6);
    }
}
