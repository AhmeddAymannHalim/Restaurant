using Restaurant.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Configurations
{
    public class MenuItemConfigurations : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.Property(p=>p.ArName).HasColumnType("nvarchar(40)");

            builder.Property(p=>p.EnName).HasColumnType("nvarchar(40)");
            builder.Property(p=>p.Description).HasColumnType("nvarchar(150)");
            builder.Property(p => p.TotalCost).HasPrecision(18, 2);
            builder.Property(p => p.Price).HasPrecision(18, 2);
            builder.Property(p => p.Benefit).HasPrecision(18, 2);
            builder.Property(p=>p.ImageUrl).HasColumnType("nvarchar(100)");
          //  builder.Property(p => p.Type).HasColumnType("nvarchar(30)");

           

        }
    }
}
