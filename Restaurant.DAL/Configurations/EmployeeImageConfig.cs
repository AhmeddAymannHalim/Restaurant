using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeImageConfig : IEntityTypeConfiguration<EmployeeImage>
{
    public void Configure(EntityTypeBuilder<EmployeeImage> builder)
    {
        builder.Property(a => a.Url).IsRequired().HasMaxLength(500);

        builder
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.EmployeeId).IsUnique();
    }
}
