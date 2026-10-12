using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class IndexSettingsConfigurations : IEntityTypeConfiguration<IndexSettings>
{
    public void Configure(EntityTypeBuilder<IndexSettings> builder)
    {
        builder.Property(a => a.ArName).HasMaxLength(50);
        builder.Property(a => a.EnName).HasMaxLength(50);
        builder.Property(a => a.ArDescription).HasColumnName("Description").HasMaxLength(800);
        builder.Property(a => a.EnDescription).HasMaxLength(800);
        builder.Property(a => a.Phone).HasMaxLength(20);
        builder.Property(a => a.Email).HasMaxLength(50);
        builder.Property(a => a.StartDay).HasMaxLength(50);
        builder.Property(a => a.EndDay).HasMaxLength(50);
        builder.Property(a => a.StartHour).HasMaxLength(50);
        builder.Property(a => a.EndHour).HasMaxLength(50);
        builder.Property(a => a.ImageUrl).HasMaxLength(100);
        builder.Property(a => a.VideoUrl).HasColumnName("VedioUrl").HasMaxLength(100);
        builder.Property(a => a.ArLocation).HasColumnName("Locaion").HasMaxLength(100);
        builder.Property(a => a.EnLocation).HasMaxLength(100);
    }
}
