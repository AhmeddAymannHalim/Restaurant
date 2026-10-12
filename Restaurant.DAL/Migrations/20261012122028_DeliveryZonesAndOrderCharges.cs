using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryZonesAndOrderCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryZoneId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "OnlineOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryZoneId",
                table: "OnlineOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Service",
                table: "OnlineOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "OnlineOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DeliveryZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryZones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryZoneId",
                table: "Orders",
                column: "DeliveryZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_DeliveryZoneId",
                table: "OnlineOrders",
                column: "DeliveryZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryZones_ArName",
                table: "DeliveryZones",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryZones_EnName",
                table: "DeliveryZones",
                column: "EnName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_DeliveryZones_DeliveryZoneId",
                table: "OnlineOrders",
                column: "DeliveryZoneId",
                principalTable: "DeliveryZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryZones_DeliveryZoneId",
                table: "Orders",
                column: "DeliveryZoneId",
                principalTable: "DeliveryZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_DeliveryZones_DeliveryZoneId",
                table: "OnlineOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryZones_DeliveryZoneId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DeliveryZones");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryZoneId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OnlineOrders_DeliveryZoneId",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryZoneId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryZoneId",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "Service",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "OnlineOrders");
        }
    }
}
