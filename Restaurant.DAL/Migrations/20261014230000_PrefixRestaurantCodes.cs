using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261014230000_PrefixRestaurantCodes")]
    public partial class PrefixRestaurantCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Items SET Code = N'RST-' + Code WHERE Code LIKE N'[PM][0-9]%' AND Code NOT LIKE N'%[^PM0-9]%';
UPDATE Units SET Code = N'RST-' + Code WHERE Code LIKE N'U[0-9]%' AND Code NOT LIKE N'%[^U0-9]%';
UPDATE Units SET Code = N'RST-BASE' WHERE Code = N'BASE';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Items SET Code = SUBSTRING(Code, 5, 50) WHERE Code LIKE N'RST-[PM][0-9]%';
UPDATE Units SET Code = SUBSTRING(Code, 5, 50) WHERE Code LIKE N'RST-U[0-9]%';
UPDATE Units SET Code = N'BASE' WHERE Code = N'RST-BASE';
");
        }
    }
}
