using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class SalaryConfig : IEntityTypeConfiguration<Salary>
{
    public void Configure(EntityTypeBuilder<Salary> builder)
    {
        builder.HasIndex(a => a.SalaryDate);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Salaries)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.SalaryCode).IsRequired().HasMaxLength(20);
        builder.Property(a => a.SalaryAmount).HasPrecision(18, 2);
    }
}
