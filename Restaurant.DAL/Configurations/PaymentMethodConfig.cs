using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class PaymentMethodConfig : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder
            .HasOne(a => a.Account)
            .WithMany()
            .HasForeignKey(a => a.AccountPayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.BillMaster)
            .WithMany(a => a.PaymentMethods)
            .HasForeignKey(a => a.BillMasterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
