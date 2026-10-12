using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ScroungeConfig : IEntityTypeConfiguration<Scrounge>
{
    public void Configure(EntityTypeBuilder<Scrounge> builder)
    {
        builder.HasIndex(a => a.Date);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Scrounges)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.Info).HasMaxLength(250);
        builder.Property(a => a.Amount).HasPrecision(18, 2);
    }
}
