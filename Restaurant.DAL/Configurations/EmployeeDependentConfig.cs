using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeDependentConfig : IEntityTypeConfiguration<EmployeeDependent>
{
    public void Configure(EntityTypeBuilder<EmployeeDependent> builder)
    {
        builder.Property(a => a.ArName).HasMaxLength(100);
        builder.Property(a => a.EnName).HasMaxLength(100);
        builder.Property(a => a.Relation).HasMaxLength(50);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Dependents)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
