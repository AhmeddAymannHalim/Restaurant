using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class TableConfig : IEntityTypeConfiguration<Restaurant.DAL.Models.Table>
{
    public void Configure(EntityTypeBuilder<Restaurant.DAL.Models.Table> builder)
    {
        builder.HasIndex(a => a.ArName).IsUnique();
        builder.HasIndex(a => a.EnName).IsUnique();

        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.NumOfPersons).IsRequired();
    }
}
