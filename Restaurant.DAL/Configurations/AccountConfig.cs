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

        builder.Property(a => a.NameAr).IsRequired().HasMaxLength(150);
        builder.Property(a => a.NameEn).IsRequired().HasMaxLength(150);

        builder
            .HasOne(a => a.ParentAccount)
            .WithMany(a => a.ChildAccounts)
            .HasForeignKey(a => a.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
