using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class CompanyConfig : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(150);

        builder
            .HasOne(a => a.Currency)
            .WithMany(a => a.Companies)
            .HasForeignKey(a => a.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
