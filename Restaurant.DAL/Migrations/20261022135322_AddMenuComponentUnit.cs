using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuComponentUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TransRate",
                table: "MenuItemComponents",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<int>(
                name: "UnitPriceId",
                table: "MenuItemComponents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemComponents_UnitPriceId",
                table: "MenuItemComponents",
                column: "UnitPriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemComponents_UnitPrices_UnitPriceId",
                table: "MenuItemComponents",
                column: "UnitPriceId",
                principalTable: "UnitPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemComponents_UnitPrices_UnitPriceId",
                table: "MenuItemComponents");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemComponents_UnitPriceId",
                table: "MenuItemComponents");

            migrationBuilder.DropColumn(
                name: "TransRate",
                table: "MenuItemComponents");

            migrationBuilder.DropColumn(
                name: "UnitPriceId",
                table: "MenuItemComponents");
        }
    }
}
