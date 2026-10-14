using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class JournalEntryConfig : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.Property(a => a.EntryNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(a => a.EntryNumber).IsUnique();
        builder.HasIndex(a => a.Date);
        builder.HasIndex(a => new { a.SourceType, a.SourceId });

        builder.Property(a => a.Description).HasMaxLength(250);

        builder
            .HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
