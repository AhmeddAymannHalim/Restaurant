using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class InvoiceItemConfig : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.Property(a => a.Quantity).HasPrecision(18, 3);

        builder
            .HasOne(a => a.Invoice)
            .WithMany(a => a.InvoiceItems)
            .HasForeignKey(a => a.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Product)
            .WithMany(a => a.InvoiceItems)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.UnitPrice)
            .WithMany()
            .HasForeignKey(a => a.UnitPriceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
