using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.ArName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(150);

        builder
            .HasOne(a => a.ParentAccount)
            .WithMany(a => a.ChildAccounts)
            .HasForeignKey(a => a.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.FinalAccount)
            .WithMany()
            .HasForeignKey(a => a.FinalAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Currency)
            .WithMany(a => a.Accounts)
            .HasForeignKey(a => a.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Employee)
            .WithMany(a => a.Accounts)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.AccountDetails)
            .WithOne(a => a.Account)
            .HasForeignKey<AccountDetails>(a => a.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
