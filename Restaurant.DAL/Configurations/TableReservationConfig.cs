using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.Configurations;

public class TableReservationConfig : IEntityTypeConfiguration<TableReservation>
{
    public void Configure(EntityTypeBuilder<TableReservation> builder)
    {
        builder.Property(a => a.GuestName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Email).HasMaxLength(150);
        builder.Property(a => a.Notes).HasMaxLength(500);
        builder.Property(a => a.RejectionReason).HasMaxLength(250);
        builder.Property(a => a.Status).HasDefaultValue(ReservationStatus.Pending);

        builder.HasIndex(a => new { a.TableId, a.ReservationDate });

        builder
            .HasOne(a => a.Table)
            .WithMany()
            .HasForeignKey(a => a.TableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
