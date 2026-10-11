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
    public class MenuItemConfigurations : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.Property(p=>p.Name).HasColumnType("nvarchar(40)");
            builder.Property(p=>p.Description).HasColumnType("nvarchar(150)");
           // builder.Property(p=>p.TotalCost).HasColumnType("dec(7,3)");
            builder.Property(p=>p.ImageUrl).HasColumnType("nvarchar(100)");
          //  builder.Property(p => p.Type).HasColumnType("nvarchar(30)");

           

        }
    }
}
