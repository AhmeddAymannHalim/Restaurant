using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ScroungeConfig : IEntityTypeConfiguration<Scrounge>
{
    public void Configure(EntityTypeBuilder<Scrounge> builder)
    {
        builder.Property(a => a.Info).HasMaxLength(250);
        builder.Property(a => a.Amount).HasPrecision(18, 2);
    }
}
