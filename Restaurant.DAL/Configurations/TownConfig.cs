using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class TownConfig : IEntityTypeConfiguration<Town>
{
    public void Configure(EntityTypeBuilder<Town> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);

        builder
            .HasOne(a => a.Governorate)
            .WithMany(a => a.Towns)
            .HasForeignKey(a => a.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
