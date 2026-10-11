using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class ExitExpenseConfig : IEntityTypeConfiguration<ExitExpense>
{
    public void Configure(EntityTypeBuilder<ExitExpense> builder)
    {
        builder.Property(a => a.Price).HasPrecision(18, 2);
    }
}
