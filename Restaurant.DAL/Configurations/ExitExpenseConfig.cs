using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ExitExpenseConfig : IEntityTypeConfiguration<ExitExpense>
{
    public void Configure(EntityTypeBuilder<ExitExpense> builder)
    {
        builder.HasIndex(a => a.Date);

        builder
            .HasOne(a => a.Expense)
            .WithMany(a => a.Expenses)
            .HasForeignKey(a => a.ExpenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.Price).HasPrecision(18, 2);
    }
}
