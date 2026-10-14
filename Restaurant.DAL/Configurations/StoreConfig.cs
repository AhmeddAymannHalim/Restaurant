using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class StoreConfig : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasIndex(a => a.ArName).IsUnique();
        builder.HasIndex(a => a.EnName).IsUnique();

        builder.Property(a => a.Code).HasMaxLength(20);
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.PhoneNumber1).HasMaxLength(20);

        builder
            .HasOne(a => a.Parent)
            .WithMany()
            .HasForeignKey(a => a.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Country)
            .WithMany()
            .HasForeignKey(a => a.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Gov)
            .WithMany()
            .HasForeignKey(a => a.GovId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Town)
            .WithMany()
            .HasForeignKey(a => a.TownId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
