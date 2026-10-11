using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class InvoiceItemConfig : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        // Store -> Invoice -> InvoiceItem and Store -> StoreProduct -> InvoiceItem would be two cascade paths.
        builder
            .HasOne(a => a.StoreProduct)
            .WithMany()
            .HasForeignKey(a => a.StoreProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
