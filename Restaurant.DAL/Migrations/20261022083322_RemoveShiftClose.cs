using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShiftClose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftCloseReports");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Cash_FinishShift",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinishShift",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Cash",
                table: "Orders",
                column: "Cash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Cash",
                table: "Orders");

            migrationBuilder.AddColumn<bool>(
                name: "FinishShift",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ShiftCloseReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CountedCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HadIncompleteOrders = table.Column<bool>(type: "bit", nullable: false),
                    Variance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftCloseReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftCloseReports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Cash_FinishShift",
                table: "Orders",
                columns: new[] { "Cash", "FinishShift" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCloseReports_ClosedAt",
                table: "ShiftCloseReports",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCloseReports_UserId",
                table: "ShiftCloseReports",
                column: "UserId");
        }
    }
}
