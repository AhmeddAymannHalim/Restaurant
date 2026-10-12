using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(a => a.ArName).IsUnique();
        builder.HasIndex(a => a.EnName).IsUnique();

        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.PhoneNumber).HasMaxLength(11);
        builder.Property(a => a.MobileNumber).HasMaxLength(11);
        builder.Property(a => a.Balance).HasPrecision(18, 2);
    }
}
