using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Restaurant.DAL;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261013120000_OrderPaymentMethod")]
    public partial class OrderPaymentMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Orders",
                type: "int",
                nullable: true);

            // Orders that were already paid before this column existed were all cash.
            migrationBuilder.Sql("UPDATE Orders SET PaymentMethod = 0 WHERE Cash = 1 AND OnAccount = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PaymentMethod", table: "Orders");
        }
    }
}
