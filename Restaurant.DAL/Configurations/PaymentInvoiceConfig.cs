using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class PaymentInvoiceSupplierConfig : IEntityTypeConfiguration<PaymentInvoiceSupplier>
{
    public void Configure(EntityTypeBuilder<PaymentInvoiceSupplier> builder)
    {
        builder.Property(a => a.ProcessCode).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Description).HasMaxLength(250);
        builder.HasIndex(a => a.ProcessCode).IsUnique();
        builder.HasIndex(a => a.Date);

        builder
            .HasOne(a => a.Supplier)
            .WithMany(a => a.PaymentInvoices)
            .HasForeignKey(a => a.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentInvoiceCustomerConfig : IEntityTypeConfiguration<PaymentInvoiceCustomer>
{
    public void Configure(EntityTypeBuilder<PaymentInvoiceCustomer> builder)
    {
        builder.Property(a => a.ProcessCode).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Description).HasMaxLength(250);
        builder.HasIndex(a => a.ProcessCode).IsUnique();
        builder.HasIndex(a => a.Date);

        builder
            .HasOne(a => a.Customer)
            .WithMany(a => a.PaymentInvoices)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
