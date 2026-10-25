using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnitConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitPrices_UnitPrices_BaseUnitPriceId",
                table: "UnitPrices");

            migrationBuilder.DropIndex(
                name: "IX_UnitPrices_BaseUnitPriceId",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "BaseRate",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "BaseUnitPriceId",
                table: "UnitPrices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseRate",
                table: "UnitPrices",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaseUnitPriceId",
                table: "UnitPrices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitPrices_BaseUnitPriceId",
                table: "UnitPrices",
                column: "BaseUnitPriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitPrices_UnitPrices_BaseUnitPriceId",
                table: "UnitPrices",
                column: "BaseUnitPriceId",
                principalTable: "UnitPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
