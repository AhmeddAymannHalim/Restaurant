using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class CountryConfig : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
    }
}

public class GovernorateConfig : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);

        builder
            .HasOne(a => a.Country)
            .WithMany(a => a.Governorates)
            .HasForeignKey(a => a.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CityConfig : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);

        builder
            .HasOne(a => a.Governorate)
            .WithMany(a => a.Cities)
            .HasForeignKey(a => a.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
