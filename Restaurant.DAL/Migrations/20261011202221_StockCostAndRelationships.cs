using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class StockCostAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Products_ProductId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_StoreProducts_StoreProductId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_UnitPrices_UnitPriceId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Stores_StoreId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Suppliers_SupplierId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemComponents_StoreProducts_StoreProductId",
                table: "MenuItemComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuSections_Kitchens_kitchenId",
                table: "MenuSections");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrderMenuItems_MenuItems_MenuItemId",
                table: "OnlineOrderMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_AspNetUsers_CustomerId",
                table: "OnlineOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMenuItems_MenuItems_MenuItemId",
                table: "OrderMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ReceiverId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_SellerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInvoiceCustomer_Customers_CustomerId",
                table: "PaymentInvoiceCustomer");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInvoiceSupplier_Suppliers_SupplierId",
                table: "PaymentInvoiceSupplier");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreConvertProducts_StoreProducts_StoreProductId",
                table: "StoreConvertProducts");

            migrationBuilder.DropIndex(
                name: "IX_StoreProducts_StoreId",
                table: "StoreProducts");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_StoreProductId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "PriceForUnit",
                table: "StoreProducts");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "StoreProducts");

            migrationBuilder.DropColumn(
                name: "StoreName",
                table: "StoreProducts");

            migrationBuilder.DropColumn(
                name: "Benefit",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "MenuItemComponents");

            migrationBuilder.DropColumn(
                name: "StoreProductId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "InvoiceItems");

            migrationBuilder.RenameColumn(
                name: "kitchenId",
                table: "MenuSections",
                newName: "KitchenId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuSections_kitchenId",
                table: "MenuSections",
                newName: "IX_MenuSections_KitchenId");

            migrationBuilder.RenameColumn(
                name: "StoreProductId",
                table: "MenuItemComponents",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItemComponents_StoreProductId",
                table: "MenuItemComponents",
                newName: "IX_MenuItemComponents_ProductId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "StoreProducts",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "StoreConvertProducts",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessCode",
                table: "PaymentInvoiceSupplier",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PaymentInvoiceSupplier",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessCode",
                table: "PaymentInvoiceCustomer",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PaymentInvoiceCustomer",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Orders",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "Orders",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OrderMenuItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "OnlineOrders",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "OnlineOrders",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "OnlineOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "OnlineOrders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OnlineOrderMenuItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceCode",
                table: "Invoices",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "InvoiceItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreProducts_StoreId_ProductId",
                table: "StoreProducts",
                columns: new[] { "StoreId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceSupplier_ProcessCode",
                table: "PaymentInvoiceSupplier",
                column: "ProcessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceCustomer_ProcessCode",
                table: "PaymentInvoiceCustomer",
                column: "ProcessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceCode",
                table: "Invoices",
                column: "InvoiceCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Products_ProductId",
                table: "InvoiceItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_UnitPrices_UnitPriceId",
                table: "InvoiceItems",
                column: "UnitPriceId",
                principalTable: "UnitPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Stores_StoreId",
                table: "Invoices",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Suppliers_SupplierId",
                table: "Invoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemComponents_Products_ProductId",
                table: "MenuItemComponents",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuSections_Kitchens_KitchenId",
                table: "MenuSections",
                column: "KitchenId",
                principalTable: "Kitchens",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrderMenuItems_MenuItems_MenuItemId",
                table: "OnlineOrderMenuItems",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_AspNetUsers_CustomerId",
                table: "OnlineOrders",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMenuItems_MenuItems_MenuItemId",
                table: "OrderMenuItems",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ReceiverId",
                table: "Orders",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_SellerId",
                table: "Orders",
                column: "SellerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInvoiceCustomer_Customers_CustomerId",
                table: "PaymentInvoiceCustomer",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInvoiceSupplier_Suppliers_SupplierId",
                table: "PaymentInvoiceSupplier",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreConvertProducts_StoreProducts_StoreProductId",
                table: "StoreConvertProducts",
                column: "StoreProductId",
                principalTable: "StoreProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Products_ProductId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_UnitPrices_UnitPriceId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Stores_StoreId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Suppliers_SupplierId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemComponents_Products_ProductId",
                table: "MenuItemComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuSections_Kitchens_KitchenId",
                table: "MenuSections");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrderMenuItems_MenuItems_MenuItemId",
                table: "OnlineOrderMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_AspNetUsers_CustomerId",
                table: "OnlineOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMenuItems_MenuItems_MenuItemId",
                table: "OrderMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ReceiverId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_SellerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInvoiceCustomer_Customers_CustomerId",
                table: "PaymentInvoiceCustomer");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInvoiceSupplier_Suppliers_SupplierId",
                table: "PaymentInvoiceSupplier");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreConvertProducts_StoreProducts_StoreProductId",
                table: "StoreConvertProducts");

            migrationBuilder.DropIndex(
                name: "IX_StoreProducts_StoreId_ProductId",
                table: "StoreProducts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInvoiceSupplier_ProcessCode",
                table: "PaymentInvoiceSupplier");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInvoiceCustomer_ProcessCode",
                table: "PaymentInvoiceCustomer");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "KitchenId",
                table: "MenuSections",
                newName: "kitchenId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuSections_KitchenId",
                table: "MenuSections",
                newName: "IX_MenuSections_kitchenId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "MenuItemComponents",
                newName: "StoreProductId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItemComponents_ProductId",
                table: "MenuItemComponents",
                newName: "IX_MenuItemComponents_StoreProductId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "StoreProducts",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceForUnit",
                table: "StoreProducts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "StoreProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreName",
                table: "StoreProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "StoreConvertProducts",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessCode",
                table: "PaymentInvoiceSupplier",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PaymentInvoiceSupplier",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessCode",
                table: "PaymentInvoiceCustomer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PaymentInvoiceCustomer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OrderMenuItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "OnlineOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "OnlineOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "OnlineOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "OnlineOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OnlineOrderMenuItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AddColumn<decimal>(
                name: "Benefit",
                table: "MenuItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "MenuItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "MenuItemComponents",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceCode",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "InvoiceItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InvoiceItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "StoreProductId",
                table: "InvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "InvoiceItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_StoreProducts_StoreId",
                table: "StoreProducts",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_StoreProductId",
                table: "InvoiceItems",
                column: "StoreProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Products_ProductId",
                table: "InvoiceItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_StoreProducts_StoreProductId",
                table: "InvoiceItems",
                column: "StoreProductId",
                principalTable: "StoreProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_UnitPrices_UnitPriceId",
                table: "InvoiceItems",
                column: "UnitPriceId",
                principalTable: "UnitPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Stores_StoreId",
                table: "Invoices",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Suppliers_SupplierId",
                table: "Invoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemComponents_StoreProducts_StoreProductId",
                table: "MenuItemComponents",
                column: "StoreProductId",
                principalTable: "StoreProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuSections_Kitchens_kitchenId",
                table: "MenuSections",
                column: "kitchenId",
                principalTable: "Kitchens",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrderMenuItems_MenuItems_MenuItemId",
                table: "OnlineOrderMenuItems",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_AspNetUsers_CustomerId",
                table: "OnlineOrders",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMenuItems_MenuItems_MenuItemId",
                table: "OrderMenuItems",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ReceiverId",
                table: "Orders",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_SellerId",
                table: "Orders",
                column: "SellerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInvoiceCustomer_Customers_CustomerId",
                table: "PaymentInvoiceCustomer",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInvoiceSupplier_Suppliers_SupplierId",
                table: "PaymentInvoiceSupplier",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreConvertProducts_StoreProducts_StoreProductId",
                table: "StoreConvertProducts",
                column: "StoreProductId",
                principalTable: "StoreProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
