using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class PosBillLineConfig : IEntityTypeConfiguration<PosBillLine>
{
    public void Configure(EntityTypeBuilder<PosBillLine> builder)
    {
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.HasOne(a => a.BillDetail).WithOne().HasForeignKey<PosBillLine>(a => a.Id).OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.PosBill)
            .WithMany(a => a.Lines)
            .HasForeignKey(a => a.PosBillId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(a => a.MenuItem)
            .WithMany()
            .HasForeignKey(a => a.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
