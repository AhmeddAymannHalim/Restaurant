using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(a => a.AccountId).IsUnique();

        builder.Property(a => a.PhoneNumber1).HasMaxLength(20);
        builder.Property(a => a.PhoneNumber2).HasMaxLength(20);
        builder.Property(a => a.DetailedAddress).HasMaxLength(250);
        builder.Property(a => a.CreditLimit).HasPrecision(18, 2);

        builder
            .HasOne(a => a.Account)
            .WithMany()
            .HasForeignKey(a => a.AccountId)
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
    }
}
