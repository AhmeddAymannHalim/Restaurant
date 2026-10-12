using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ProductCategoriesAndNameConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConvertCode",
                table: "StoreConverts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnName",
                table: "MenuItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArName",
                table: "MenuItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitPrices_ArName",
                table: "UnitPrices",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitPrices_EnName",
                table: "UnitPrices",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_ArName",
                table: "Tables",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_EnName",
                table: "Tables",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ArName",
                table: "Suppliers",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_EnName",
                table: "Suppliers",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ArName",
                table: "Stores",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_EnName",
                table: "Stores",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreConverts_ConvertCode",
                table: "StoreConverts",
                column: "ConvertCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryTypes_ArName",
                table: "SalaryTypes",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryTypes_EnName",
                table: "SalaryTypes",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ArName",
                table: "Products",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_EnName",
                table: "Products",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuSections_ArName",
                table: "MenuSections",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuSections_EnName",
                table: "MenuSections",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kitchens_ArName",
                table: "Kitchens",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kitchens_EnName",
                table: "Kitchens",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ArName",
                table: "Jobs",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_EnName",
                table: "Jobs",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ArName",
                table: "Expenses",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_EnName",
                table: "Expenses",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ArName",
                table: "Customers",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_EnName",
                table: "Customers",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ArName",
                table: "Branches",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_EnName",
                table: "Branches",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ArName",
                table: "ProductCategories",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_EnName",
                table: "ProductCategories",
                column: "EnName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_UnitPrices_ArName",
                table: "UnitPrices");

            migrationBuilder.DropIndex(
                name: "IX_UnitPrices_EnName",
                table: "UnitPrices");

            migrationBuilder.DropIndex(
                name: "IX_Tables_ArName",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Tables_EnName",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_ArName",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_EnName",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ArName",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_EnName",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_StoreConverts_ConvertCode",
                table: "StoreConverts");

            migrationBuilder.DropIndex(
                name: "IX_SalaryTypes_ArName",
                table: "SalaryTypes");

            migrationBuilder.DropIndex(
                name: "IX_SalaryTypes_EnName",
                table: "SalaryTypes");

            migrationBuilder.DropIndex(
                name: "IX_Products_ArName",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_EnName",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_MenuSections_ArName",
                table: "MenuSections");

            migrationBuilder.DropIndex(
                name: "IX_MenuSections_EnName",
                table: "MenuSections");

            migrationBuilder.DropIndex(
                name: "IX_Kitchens_ArName",
                table: "Kitchens");

            migrationBuilder.DropIndex(
                name: "IX_Kitchens_EnName",
                table: "Kitchens");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ArName",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_EnName",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ArName",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_EnName",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ArName",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_EnName",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ArName",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_EnName",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "ConvertCode",
                table: "StoreConverts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "EnName",
                table: "MenuItems",
                type: "nvarchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ArName",
                table: "MenuItems",
                type: "nvarchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
