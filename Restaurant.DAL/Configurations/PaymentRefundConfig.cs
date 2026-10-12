using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class PaymentRefundConfig : IEntityTypeConfiguration<PaymentRefund>
{
    public void Configure(EntityTypeBuilder<PaymentRefund> builder)
    {
        builder.Property(a => a.Amount).HasPrecision(18, 2);
        builder.Property(a => a.Reason).IsRequired().HasMaxLength(250);

        builder.HasIndex(a => a.OriginalPaymentId);
        builder.HasIndex(a => a.RequestedAt);

        builder
            .HasOne(a => a.OriginalPayment)
            .WithMany()
            .HasForeignKey(a => a.OriginalPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.RequestedByUser)
            .WithMany()
            .HasForeignKey(a => a.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.ApprovedByUser)
            .WithMany()
            .HasForeignKey(a => a.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
