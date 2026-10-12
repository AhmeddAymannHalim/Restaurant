using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ShiftCloseReportConfig : IEntityTypeConfiguration<ShiftCloseReport>
{
    public void Configure(EntityTypeBuilder<ShiftCloseReport> builder)
    {
        builder.HasIndex(a => a.ClosedAt);
        builder.Property(a => a.ExpectedCash).HasPrecision(18, 2);
        builder.Property(a => a.CountedCash).HasPrecision(18, 2);
        builder.Property(a => a.Variance).HasPrecision(18, 2);
    }
}
