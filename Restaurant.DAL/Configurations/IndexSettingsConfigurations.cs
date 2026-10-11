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
    public class IndexSettingsConfigurations : IEntityTypeConfiguration<IndexSettings>
    {
        public void Configure(EntityTypeBuilder<IndexSettings> builder)
        {
            builder.Property(p=>p.StartHour).HasColumnType("nvarchar(50)");
            builder.Property(p => p.StartDay).HasColumnType("nvarchar(50)");
            builder.Property(p => p.EndHour).HasColumnType("nvarchar(50)");
            builder.Property(p => p.ArName).HasColumnType("nvarchar(50)");

            builder.Property(p => p.EnName).HasColumnType("nvarchar(50)");
            builder.Property(p => p.Description).HasColumnType("nvarchar(800)");
            builder.Property(p => p.EndDay).HasColumnType("nvarchar(50)");
            builder.Property(p => p.Phone).HasColumnType("nvarchar(20)");
            builder.Property(p => p.ImageUrl).HasColumnType("nvarchar(100)");
            builder.Property(p => p.VedioUrl).HasColumnType("nvarchar(100)");
            builder.Property(p => p.Locaion).HasColumnType("nvarchar(100)");
            builder.Property(p => p.Email).HasColumnType("nvarchar(50)");
        }
    }
}
