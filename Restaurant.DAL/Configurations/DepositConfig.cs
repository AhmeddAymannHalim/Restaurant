using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class DepositConfig : IEntityTypeConfiguration<Deposit>
{
    public void Configure(EntityTypeBuilder<Deposit> builder)
    {
        builder.HasIndex(a => a.CreatedTime);
        builder.Property(a => a.ProcessCode).IsRequired().HasMaxLength(20);
        builder.Property(a => a.DeposerName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Amount).HasPrecision(18, 2);
        builder.Property(a => a.Description).HasMaxLength(250);
    }
}
