using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class OnAccountIngredientsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "PayTypes");

            migrationBuilder.DropTable(
                name: "PrinterForSections");

            migrationBuilder.DropTable(
                name: "SellTypes");

            migrationBuilder.DropTable(
                name: "AccountStates");

            migrationBuilder.DropTable(
                name: "AccountTypes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StoreProducts",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OnAccount",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OrderIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderMenuItemId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderIngredients_OrderMenuItems_OrderMenuItemId",
                        column: x => x.OrderMenuItemId,
                        principalTable: "OrderMenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderIngredients_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderIngredients_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scrounges_Date",
                table: "Scrounges",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_SalaryDate",
                table: "Salaries",
                column: "SalaryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceSupplier_Date",
                table: "PaymentInvoiceSupplier",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceCustomer_Date",
                table: "PaymentInvoiceCustomer",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Cash_FinishShift",
                table: "Orders",
                columns: new[] { "Cash", "FinishShift" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceDate",
                table: "Invoices",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExitExpenses_Date",
                table: "ExitExpenses",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_CreatedTime",
                table: "Deposits",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_OrderIngredients_OrderMenuItemId",
                table: "OrderIngredients",
                column: "OrderMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderIngredients_ProductId",
                table: "OrderIngredients",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderIngredients_StoreId",
                table: "OrderIngredients",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderIngredients");

            migrationBuilder.DropIndex(
                name: "IX_Scrounges_Date",
                table: "Scrounges");

            migrationBuilder.DropIndex(
                name: "IX_Salaries_SalaryDate",
                table: "Salaries");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInvoiceSupplier_Date",
                table: "PaymentInvoiceSupplier");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInvoiceCustomer_Date",
                table: "PaymentInvoiceCustomer");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Cash_FinishShift",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceDate",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_ExitExpenses_Date",
                table: "ExitExpenses");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_CreatedTime",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StoreProducts");

            migrationBuilder.DropColumn(
                name: "OnAccount",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "AccountStates",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    ArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountTypes",
                columns: table => new
                {
                    AccTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    AccTypeArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccTypeEnName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTypes", x => x.AccTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    CurrencyId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "PayTypes",
                columns: table => new
                {
                    PayTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayTypeArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayTypeEnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayTypeNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayTypes", x => x.PayTypeId);
                });

            migrationBuilder.CreateTable(
                name: "PrinterForSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Printer = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterForSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrinterForSections_MenuSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "MenuSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SellTypes",
                columns: table => new
                {
                    SellTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellDefault = table.Column<bool>(type: "bit", nullable: true),
                    SellPlusPrice = table.Column<double>(type: "float", nullable: true),
                    SellTypeArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SellTypeEnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowOrNot = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellTypes", x => x.SellTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccStateId = table.Column<byte>(type: "tinyint", nullable: false),
                    AccTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrencyId = table.Column<short>(type: "smallint", nullable: false),
                    AccArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccCheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccCredit = table.Column<double>(type: "float", nullable: false),
                    AccCustomerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccDebit = table.Column<double>(type: "float", nullable: false),
                    AccDebitOrCreditOrWithout = table.Column<byte>(type: "tinyint", nullable: false),
                    AccEnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccLevel = table.Column<int>(type: "int", nullable: true),
                    AccMaxDebit = table.Column<double>(type: "float", nullable: true),
                    AccMustCostCenter = table.Column<bool>(type: "bit", nullable: true),
                    AccNsons = table.Column<short>(type: "smallint", nullable: false),
                    AccRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccTurnningYesOrNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class = table.Column<bool>(type: "bit", nullable: true),
                    FinalAccId = table.Column<int>(type: "int", nullable: true),
                    ParentAccId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccId);
                    table.ForeignKey(
                        name: "FK_Accounts_AccountStates_AccStateId",
                        column: x => x.AccStateId,
                        principalTable: "AccountStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_AccountTypes_AccTypeId",
                        column: x => x.AccTypeId,
                        principalTable: "AccountTypes",
                        principalColumn: "AccTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccStateId",
                table: "Accounts",
                column: "AccStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccTypeId",
                table: "Accounts",
                column: "AccTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CurrencyId",
                table: "Accounts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterForSections_SectionId",
                table: "PrinterForSections",
                column: "SectionId");
        }
    }
}
