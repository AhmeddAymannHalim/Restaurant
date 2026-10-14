using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeePhoneConfig : IEntityTypeConfiguration<EmployeePhone>
{
    public void Configure(EntityTypeBuilder<EmployeePhone> builder)
    {
        builder.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(20);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Phones)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EmployeeId, a.PhoneNumber }).IsUnique();
    }
}
