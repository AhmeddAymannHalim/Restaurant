using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeAndAuditToDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuSections_Kitchens_KitchenId",
                table: "MenuSections");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "UnitPrices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UnitPrices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "UnitPrices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UnitPrices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "UnitPrices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "UnitPrices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UnitPrices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "UnitPrices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tables",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Tables",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Tables",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tables",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SellTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "ProductCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProductCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "MenuSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MenuSections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "MenuSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MenuSections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "MenuSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "MenuSections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MenuSections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "MenuSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "MenuItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "LeaveTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Kitchens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Kitchens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Kitchens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Kitchens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Kitchens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Kitchens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Kitchens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Kitchens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "JobTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InsuranceCompanies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InsuranceCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "DeliveryZones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DeliveryZones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "DeliveryZones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DeliveryZones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "DeliveryZones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "DeliveryZones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DeliveryZones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "DeliveryZones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "DeductionTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "CustomerTiers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ContractTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Classifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AllowanceTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuSections_Kitchens_KitchenId",
                table: "MenuSections",
                column: "KitchenId",
                principalTable: "Kitchens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // existing rows get their Id as code so every definition shows a code
            migrationBuilder.Sql("UPDATE [AllowanceTypes] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [Classifications] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [ContractTypes] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [CustomerTiers] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [DeductionTypes] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [DeliveryZones] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [Departments] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [Expenses] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [InsuranceCategories] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [InsuranceCompanies] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [JobTypes] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [Kitchens] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [LeaveTypes] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [MenuItems] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [MenuSections] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [ProductCategories] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [Products] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [SellTypes] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [Tables] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
            migrationBuilder.Sql("UPDATE [UnitPrices] SET [Code] = CAST([Id] AS nvarchar(20)) WHERE [Code] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuSections_Kitchens_KitchenId",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "State",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "UnitPrices");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SellTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "State",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "State",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "State",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Kitchens");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "JobTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InsuranceCompanies");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InsuranceCategories");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "State",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "DeliveryZones");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "DeductionTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "CustomerTiers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ContractTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Classifications");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AllowanceTypes");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuSections_Kitchens_KitchenId",
                table: "MenuSections",
                column: "KitchenId",
                principalTable: "Kitchens",
                principalColumn: "Id");
        }
    }
}
