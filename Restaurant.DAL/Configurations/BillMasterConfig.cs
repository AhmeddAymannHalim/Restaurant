using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class BillMasterConfig : IEntityTypeConfiguration<BillMaster>
{
    public void Configure(EntityTypeBuilder<BillMaster> builder)
    {
        builder.Property(a => a.BillCost).HasPrecision(18, 6);
        builder.Property(a => a.BillProfit).HasPrecision(18, 6);
    }
}
