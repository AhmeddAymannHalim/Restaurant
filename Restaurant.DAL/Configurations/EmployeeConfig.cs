using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.Property(a => a.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(a => a.LastName).IsRequired().HasMaxLength(50);
        builder.Property(a => a.PhoneNumber).HasMaxLength(11);
        builder.Property(a => a.MobileNumber).IsRequired().HasMaxLength(11);
        builder.Property(a => a.Country).IsRequired().HasMaxLength(30);
        builder.Property(a => a.City).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Street).IsRequired().HasMaxLength(100);
        builder.Property(a => a.NationalID).IsRequired().HasMaxLength(14);
        builder.Property(a => a.UserType).HasMaxLength(100);
        builder.Property(a => a.Salary).HasPrecision(18, 2);

        builder
            .HasOne(a => a.SalaryType)
            .WithMany()
            .HasForeignKey(a => a.SalaryTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(a => a.FullName);
        builder.Ignore(a => a.HasAccount);
    }
}
