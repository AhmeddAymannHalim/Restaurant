using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class JobConfig : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasIndex(a => a.ArName).IsUnique();
        builder.HasIndex(a => a.EnName).IsUnique();

        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.HasMany(a => a.Employees).WithOne(e => e.Job).HasForeignKey(e => e.JobId).OnDelete(DeleteBehavior.Restrict);
    }
}
