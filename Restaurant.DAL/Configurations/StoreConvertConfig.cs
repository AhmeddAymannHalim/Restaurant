using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class StoreConvertConfig : IEntityTypeConfiguration<StoreConvert>
{
    public void Configure(EntityTypeBuilder<StoreConvert> builder)
    {
        builder.Property(a => a.ConvertCode).IsRequired().HasMaxLength(30);
        builder.HasIndex(a => a.ConvertCode).IsUnique();

        builder
            .HasOne(a => a.FromStore)
            .WithMany()
            .HasForeignKey(a => a.FromStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.ToStore)
            .WithMany()
            .HasForeignKey(a => a.ToStoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
