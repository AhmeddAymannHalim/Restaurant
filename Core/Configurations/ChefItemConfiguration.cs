using Restaurant.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Configurations
{
    public class ChefItemConfiguration : IEntityTypeConfiguration<ChefItem>
    {
        public void Configure(EntityTypeBuilder<ChefItem> builder)
        {
            builder.Property(p=>p.Name).HasColumnType("nvarchar(30)");
            builder.Property(p=>p.Jop).HasColumnType("nvarchar(40)");
            builder.Property(p=>p.ImageUrl).HasColumnType("nvarchar(100)");
        }
    }
}
