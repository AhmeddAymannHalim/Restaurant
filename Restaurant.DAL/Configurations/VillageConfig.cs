using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class VillageConfig : IEntityTypeConfiguration<Village>
{
    public void Configure(EntityTypeBuilder<Village> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);

        builder
            .HasOne(a => a.Town)
            .WithMany(a => a.Villages)
            .HasForeignKey(a => a.TownId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
