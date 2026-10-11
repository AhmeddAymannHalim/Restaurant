using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Configurations
{
    public class BrancheConfiguration : IEntityTypeConfiguration<Branche>
    {
        public void Configure(EntityTypeBuilder<Branche> builder)
        {
            builder.Property(p => p.Name).HasColumnType("nvarchar(150)").IsRequired();
            builder.Property(p => p.Address).HasColumnType("nvarchar(250)").IsRequired(); ;
            builder.Property(p => p.Phone1).HasColumnType("nvarchar(11)");
            builder.Property(p => p.Phone2).HasColumnType("nvarchar(11)");
            builder.Property(p => p.Mobile1).HasColumnType("nvarchar(11)");
            builder.Property(p => p.Mobile2).HasColumnType("nvarchar(11)");
            builder.Property(p => p.Code).HasColumnType("nvarchar(20)");
        }
    }
}
