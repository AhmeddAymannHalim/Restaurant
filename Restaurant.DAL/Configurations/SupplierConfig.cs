using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class SupplierConfig : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.PhoneNumber).HasMaxLength(11);
        builder.Property(a => a.MobileNumber).HasMaxLength(11);
        builder.Property(a => a.Balance).HasPrecision(18, 2);
        builder.Property(a => a.LastBalance).HasPrecision(18, 2);
    }
}
