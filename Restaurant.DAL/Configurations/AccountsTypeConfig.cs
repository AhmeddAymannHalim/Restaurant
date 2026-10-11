using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Configurations;

public class AccountsTypeConfig : IEntityTypeConfiguration<AccountsType>
{
    public void Configure(EntityTypeBuilder<AccountsType> builder)
    {
        builder.HasKey(a => a.AccTypeId);
    }
}
