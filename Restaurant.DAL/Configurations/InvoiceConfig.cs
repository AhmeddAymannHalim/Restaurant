using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class InvoiceConfig : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(a => a.InvoiceCode).IsRequired().HasMaxLength(30);
        builder.HasIndex(a => a.InvoiceCode).IsUnique();
        builder.HasIndex(a => a.InvoiceDate);

        builder
            .HasOne(a => a.Supplier)
            .WithMany(a => a.Invoices)
            .HasForeignKey(a => a.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Customer)
            .WithMany(a => a.Invoices)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.BillMaster)
            .WithMany()
            .HasForeignKey(a => a.BillMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Store)
            .WithMany(a => a.Invoices)
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
