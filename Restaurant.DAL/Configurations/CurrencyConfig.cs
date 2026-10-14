using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class CurrencyConfig : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.Property(a => a.Code)
               .HasMaxLength(20);

        builder.Property(a => a.ArName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EnName)
            .IsRequired()
            .HasMaxLength(100);
    }
}
