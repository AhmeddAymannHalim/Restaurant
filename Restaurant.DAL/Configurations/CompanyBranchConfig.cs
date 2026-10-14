using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class CompanyBranchConfig : IEntityTypeConfiguration<CompanyBranch>
{
    public void Configure(EntityTypeBuilder<CompanyBranch> builder)
    {
        builder.HasIndex(a => a.ArName).IsUnique();
        builder.HasIndex(a => a.EnName).IsUnique();

        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Address).HasMaxLength(250);
        builder.Property(a => a.Phone1).HasMaxLength(11);
        builder.Property(a => a.Phone2).HasMaxLength(11);
        builder.Property(a => a.Mobile1).HasMaxLength(11);
        builder.Property(a => a.Mobile2).HasMaxLength(11);

        builder
            .HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Currency)
            .WithMany(a => a.CompanyBranches)
            .HasForeignKey(a => a.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
