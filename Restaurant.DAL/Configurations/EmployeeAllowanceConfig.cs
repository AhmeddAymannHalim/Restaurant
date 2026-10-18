using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeAllowanceConfig : IEntityTypeConfiguration<EmployeeAllowance>
{
    public void Configure(EntityTypeBuilder<EmployeeAllowance> builder)
    {
        builder.Property(a => a.ArName).HasMaxLength(100);
        builder.Property(a => a.EnName).HasMaxLength(100);
        builder.Property(a => a.Amount).HasPrecision(18, 2);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Allowances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.AllowanceType)
            .WithMany()
            .HasForeignKey(a => a.AllowanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
