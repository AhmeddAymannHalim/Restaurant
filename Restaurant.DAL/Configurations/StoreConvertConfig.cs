using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class StoreConvertConfig : IEntityTypeConfiguration<StoreConvert>
{
    public void Configure(EntityTypeBuilder<StoreConvert> builder)
    {
        // Two foreign keys point to Stores, SQL Server rejects cascade on both (multiple cascade paths).
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
