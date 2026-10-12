using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class WebsiteBilingualCartAndSafeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExitExpenses_Expenses_ExpenseId",
                table: "ExitExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuSections_MenuSectionId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Stores_StoreId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Salaries_Employees_EmployeeId",
                table: "Salaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Scrounges_Employees_EmployeeId",
                table: "Scrounges");

            migrationBuilder.AddColumn<int>(
                name: "TableId",
                table: "OnlineOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnDescription",
                table: "MenuItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnDescription",
                table: "IndexSettings",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnLocation",
                table: "IndexSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_TableId",
                table: "OnlineOrders",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExitExpenses_Expenses_ExpenseId",
                table: "ExitExpenses",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuSections_MenuSectionId",
                table: "MenuItems",
                column: "MenuSectionId",
                principalTable: "MenuSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Stores_StoreId",
                table: "MenuItems",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_Tables_TableId",
                table: "OnlineOrders",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Salaries_Employees_EmployeeId",
                table: "Salaries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scrounges_Employees_EmployeeId",
                table: "Scrounges",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExitExpenses_Expenses_ExpenseId",
                table: "ExitExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuSections_MenuSectionId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Stores_StoreId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_Tables_TableId",
                table: "OnlineOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Salaries_Employees_EmployeeId",
                table: "Salaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Scrounges_Employees_EmployeeId",
                table: "Scrounges");

            migrationBuilder.DropIndex(
                name: "IX_OnlineOrders_TableId",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "EnDescription",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "EnDescription",
                table: "IndexSettings");

            migrationBuilder.DropColumn(
                name: "EnLocation",
                table: "IndexSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_ExitExpenses_Expenses_ExpenseId",
                table: "ExitExpenses",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuSections_MenuSectionId",
                table: "MenuItems",
                column: "MenuSectionId",
                principalTable: "MenuSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Stores_StoreId",
                table: "MenuItems",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Salaries_Employees_EmployeeId",
                table: "Salaries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scrounges_Employees_EmployeeId",
                table: "Scrounges",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
