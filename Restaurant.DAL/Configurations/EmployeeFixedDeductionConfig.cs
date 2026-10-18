using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeFixedDeductionConfig : IEntityTypeConfiguration<EmployeeFixedDeduction>
{
    public void Configure(EntityTypeBuilder<EmployeeFixedDeduction> builder)
    {
        builder.Property(a => a.ArName).HasMaxLength(100);
        builder.Property(a => a.EnName).HasMaxLength(100);
        builder.Property(a => a.Amount).HasPrecision(18, 2);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.FixedDeductions)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.DeductionType)
            .WithMany()
            .HasForeignKey(a => a.DeductionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
