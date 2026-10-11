using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Data.Migrations
{
    public partial class EditScroungeAndExpense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Scrounges",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Kitchens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ExitExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExitExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExitExpenses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExitExpenses_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scrounges_UserId",
                table: "Scrounges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExitExpenses_ExpenseId",
                table: "ExitExpenses",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExitExpenses_UserId",
                table: "ExitExpenses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scrounges_AspNetUsers_UserId",
                table: "Scrounges",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scrounges_AspNetUsers_UserId",
                table: "Scrounges");

            migrationBuilder.DropTable(
                name: "ExitExpenses");

            migrationBuilder.DropIndex(
                name: "IX_Scrounges_UserId",
                table: "Scrounges");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Scrounges");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Kitchens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
