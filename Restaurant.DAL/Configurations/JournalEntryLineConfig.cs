using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class JournalEntryLineConfig : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.Property(a => a.Debit).HasPrecision(18, 2);
        builder.Property(a => a.Credit).HasPrecision(18, 2);
        builder.Property(a => a.Description).HasMaxLength(250);

        builder
            .HasOne(a => a.JournalEntry)
            .WithMany(a => a.Lines)
            .HasForeignKey(a => a.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Account)
            .WithMany(a => a.JournalEntryLines)
            .HasForeignKey(a => a.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
