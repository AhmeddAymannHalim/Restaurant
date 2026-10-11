using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class BranchConfig : IEntityTypeConfiguration<Branche>
{
    public void Configure(EntityTypeBuilder<Branche> builder)
    {
        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.Property(a => a.ArName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Address).IsRequired().HasMaxLength(250);
        builder.Property(a => a.Phone1).IsRequired().HasMaxLength(11);
        builder.Property(a => a.Phone2).HasMaxLength(11);
        builder.Property(a => a.Mobile1).HasMaxLength(11);
        builder.Property(a => a.Mobile2).HasMaxLength(11);
        builder.HasMany(a => a.Employees).WithOne(e => e.Branche).HasForeignKey(e => e.BrancheId).OnDelete(DeleteBehavior.Restrict);
    }
}
