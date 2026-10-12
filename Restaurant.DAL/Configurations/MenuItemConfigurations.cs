using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class MenuItemConfigurations : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ArDescription).HasColumnName("Description").HasMaxLength(150);
        builder.Property(a => a.EnDescription).HasMaxLength(150);
        builder.Property(a => a.ShowOnWebsite).HasColumnName("IsReady");
        builder.Property(a => a.ImageUrl).HasMaxLength(100);
        builder.Property(a => a.Price).HasPrecision(18, 2);

        builder
            .HasOne(a => a.MenuSection)
            .WithMany(a => a.MenuItems)
            .HasForeignKey(a => a.MenuSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Store)
            .WithMany()
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
