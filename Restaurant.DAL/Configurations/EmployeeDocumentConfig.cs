using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeDocumentConfig : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(150);
        builder.Property(a => a.Url).HasMaxLength(500);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Documents)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
