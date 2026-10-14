using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeLeaveConfig : IEntityTypeConfiguration<EmployeeLeave>
{
    public void Configure(EntityTypeBuilder<EmployeeLeave> builder)
    {
        builder.Property(a => a.Notes).HasMaxLength(500);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Leaves)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.LeaveType)
            .WithMany()
            .HasForeignKey(a => a.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
