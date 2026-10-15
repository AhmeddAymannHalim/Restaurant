using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PhoneNumber).HasMaxLength(20);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.IDNumber).HasMaxLength(20);
        builder.Property(a => a.Salary).HasPrecision(18, 2);

        builder
            .HasMany(a => a.Accounts)
            .WithOne(a => a.Employee)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Account)
            .WithMany()
            .HasForeignKey(a => a.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Job)
            .WithMany(a => a.Employees)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Nationality)
            .WithMany()
            .HasForeignKey(a => a.NationalityId)
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

        builder.Ignore(a => a.HasLoginAccount);
        builder.Ignore(a => a.Image);
        builder.Ignore(a => a.PhoneNumbers);
        builder.Ignore(a => a.ExistingDocumentUrls);
    }
}
