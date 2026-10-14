using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PosBillsReplaceOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PosBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Service = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Cash = table.Column<bool>(type: "bit", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: true),
                    OnAccount = table.Column<bool>(type: "bit", nullable: false),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    FinishShift = table.Column<bool>(type: "bit", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TableId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    DeliveryZoneId = table.Column<int>(type: "int", nullable: true),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosBills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosBills_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosBills_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosBills_BillMasters_Id",
                        column: x => x.Id,
                        principalTable: "BillMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosBills_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosBills_DeliveryZones_DeliveryZoneId",
                        column: x => x.DeliveryZoneId,
                        principalTable: "DeliveryZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosBills_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosBills_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosBillLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PosBillId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosBillLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosBillLines_BillDetails_Id",
                        column: x => x.Id,
                        principalTable: "BillDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosBillLines_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosBillLines_PosBills_PosBillId",
                        column: x => x.PosBillId,
                        principalTable: "PosBills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PosBillIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosBillLineId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosBillIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosBillIngredients_PosBillLines_PosBillLineId",
                        column: x => x.PosBillLineId,
                        principalTable: "PosBillLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosBillIngredients_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosBillIngredients_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PosBillIngredients_PosBillLineId",
                table: "PosBillIngredients",
                column: "PosBillLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBillIngredients_ProductId",
                table: "PosBillIngredients",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBillIngredients_StoreId",
                table: "PosBillIngredients",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBillLines_MenuItemId",
                table: "PosBillLines",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBillLines_PosBillId",
                table: "PosBillLines",
                column: "PosBillId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_Cash_FinishShift",
                table: "PosBills",
                columns: new[] { "Cash", "FinishShift" });

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_CustomerId",
                table: "PosBills",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_DeliveryZoneId",
                table: "PosBills",
                column: "DeliveryZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_EmployeeId",
                table: "PosBills",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_OrderCode",
                table: "PosBills",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_ReceiverId",
                table: "PosBills",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_SellerId",
                table: "PosBills",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_PosBills_TableId",
                table: "PosBills",
                column: "TableId");

            // Cashier orders become ERP sales bills: every order turns into a BillMaster (bill setting POS) with a BillDetail per
            // order line, and the cashier-only columns move to PosBills/PosBillLines/PosBillIngredients. Orders collected before this
            // migration had no journal entry and keep none (their bills are flagged as not posted).
            migrationBuilder.Sql(@"SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT 1 FROM BillSettings WHERE Code = N'POS')
    INSERT INTO BillSettings (Code, ArName, EnName, CreatedAt) VALUES (N'POS', N'فاتورة كاشير', N'Cashier sales invoice', GETDATE());

INSERT INTO Items (Code, ArName, EnName, CreatedAt, HasProduction, HasExpiry, HasSerialInput, HasSerialOutput, HasVat, StopTrading,
                   WeightInGrams, TaxPercentage, DiscountPercentage, WeightedAverageQuantityOnHand, HasWeightedAverageHistory,
                   RequiresCostingRebuild, RequiresAverageRecalculation, EtaItemType)
SELECT N'M' + CAST(m.Id AS nvarchar(20)), m.ArName, m.EnName, GETDATE(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
FROM MenuItems m
WHERE m.Id IN (SELECT MenuItemId FROM OrderMenuItems)
  AND NOT EXISTS (SELECT 1 FROM Items i WHERE i.Code = N'M' + CAST(m.Id AS nvarchar(20)));

CREATE TABLE #BillMap (OrderId int NOT NULL PRIMARY KEY, BillId int NOT NULL);
MERGE BillMasters AS target
USING (SELECT o.Id AS OrderId, o.OrderCode, o.OrderDate, o.Discount, o.Service, o.Tax, o.DeliveryFee, o.TotalPrice, o.Cash, o.OnAccount, o.PhoneNumber,
              ISNULL((SELECT SUM(l.Price) FROM OrderMenuItems l WHERE l.OrderId = o.Id), 0) AS Sub,
              ISNULL((SELECT SUM(l.UnitCost * l.Quantity) FROM OrderMenuItems l WHERE l.OrderId = o.Id), 0) AS Cost,
              CAST(ROW_NUMBER() OVER (PARTITION BY CAST(o.OrderDate AS date) ORDER BY o.Id) AS int) AS DailyNo
       FROM Orders o) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (Code, CodeWithoutSuffix, Date, CurrencyRate, BillCost, BillProfit, TotalAmountBeforeDiscountOrExtra, TotalItemsDiscountValue,
            TotalItemsExtraValue, TotalItemsAmountAfterDiscountAndExtra, BillDiscountPercentage, BillDiscountValue, BillExtraValue,
            BillExtraPercentage, TotalDiscountValueOnBillAndItems, TotalExtraValueOnBillAndItems, TotalBillAmountAfterDiscountAndExtra,
            PayType, IsStorageEffected, IsEntryPosted, IsCostingApplied, BillDailyNo, AmountPaided, CasherCurrentPaidedAmount,
            CasherDifferenceAmount, CasherRemainingAmount, CustomerOrSupplierPhoneNumber, ZatcaInvType, IsExport, IsSentToZatca, IsSentToEta,
            BillSettingId, CreatedAt)
    VALUES (src.OrderCode, src.OrderCode, src.OrderDate, 1,
            CASE WHEN ABS(src.Cost) > 999999999999 THEN NULL ELSE src.Cost END,
            CASE WHEN ABS(src.TotalPrice - src.Cost) > 999999999999 THEN NULL ELSE src.TotalPrice - src.Cost END, src.Sub, 0,
            0, src.Sub, 0, src.Discount, src.Service + src.Tax + src.DeliveryFee,
            0, src.Discount, src.Service + src.Tax + src.DeliveryFee, src.TotalPrice,
            CASE WHEN src.OnAccount = 1 THEN 2 ELSE 1 END, 0, 0, 0, src.DailyNo,
            CASE WHEN src.Cash = 1 AND src.OnAccount = 0 THEN src.TotalPrice ELSE 0 END, 0,
            0, CASE WHEN src.Cash = 1 AND src.OnAccount = 0 THEN 0 ELSE src.TotalPrice END, src.PhoneNumber, 0, 0, 0, 0,
            (SELECT TOP 1 Id FROM BillSettings WHERE Code = N'POS'), GETDATE())
OUTPUT src.OrderId, inserted.Id INTO #BillMap (OrderId, BillId);

CREATE TABLE #DetailMap (LineId int NOT NULL PRIMARY KEY, DetailId int NOT NULL);
MERGE BillDetails AS target
USING (SELECT l.Id AS LineId, bm.BillId, l.Quantity, l.PriceForUnit, l.UnitCost, l.Price, mi.StoreId,
              (SELECT TOP 1 i.Id FROM Items i WHERE i.Code = N'M' + CAST(l.MenuItemId AS nvarchar(20))) AS ItemId,
              CAST(CASE WHEN ROW_NUMBER() OVER (PARTITION BY l.OrderId ORDER BY l.Id) > 255 THEN 255
                        ELSE ROW_NUMBER() OVER (PARTITION BY l.OrderId ORDER BY l.Id) END AS tinyint) AS RowNo
       FROM OrderMenuItems l
       JOIN #BillMap bm ON bm.OrderId = l.OrderId
       JOIN MenuItems mi ON mi.Id = l.MenuItemId) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (RowNumber, Quantity, UnitPrice, TransRate, TotalAmount, UnitCostAtPosting, BillMasterId, ItemId, StoreId, CreatedAt)
    VALUES (src.RowNo, src.Quantity, src.PriceForUnit, 1, src.Price, src.UnitCost, src.BillId, src.ItemId, src.StoreId, GETDATE())
OUTPUT src.LineId, inserted.Id INTO #DetailMap (LineId, DetailId);

INSERT INTO PosBills (Id, OrderCode, Service, Tax, Type, SellerId, ReceiverId, Cash, PaymentMethod, OnAccount, Complete, FinishShift,
                      CustomerName, TableId, CustomerId, EmployeeId, Address, PhoneNumber, DeliveryZoneId, DeliveryFee)
SELECT bm.BillId, o.OrderCode, o.Service, o.Tax, o.Type, o.SellerId, o.ReceiverId, o.Cash, o.PaymentMethod, o.OnAccount, o.Complete, o.FinishShift,
       o.CustomerName, o.TableId, o.CustomerId, o.EmployeeId, o.Address, o.PhoneNumber, o.DeliveryZoneId, o.DeliveryFee
FROM Orders o JOIN #BillMap bm ON bm.OrderId = o.Id;

INSERT INTO PosBillLines (Id, PosBillId, MenuItemId)
SELECT dm.DetailId, bm.BillId, l.MenuItemId
FROM OrderMenuItems l
JOIN #DetailMap dm ON dm.LineId = l.Id
JOIN #BillMap bm ON bm.OrderId = l.OrderId;

INSERT INTO PosBillIngredients (PosBillLineId, StoreId, ProductId, Quantity)
SELECT dm.DetailId, g.StoreId, g.ProductId, g.Quantity
FROM OrderIngredients g JOIN #DetailMap dm ON dm.LineId = g.OrderMenuItemId;

DROP TABLE #DetailMap;
DROP TABLE #BillMap;");

            migrationBuilder.DropTable(
                name: "OrderIngredients");

            migrationBuilder.DropTable(
                name: "OrderMenuItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PosBillIngredients");

            migrationBuilder.DropTable(
                name: "PosBillLines");

            migrationBuilder.DropTable(
                name: "PosBills");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    DeliveryZoneId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SellerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TableId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Cash = table.Column<bool>(type: "bit", nullable: false),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinishShift = table.Column<bool>(type: "bit", nullable: false),
                    OnAccount = table.Column<bool>(type: "bit", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Service = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_DeliveryZones_DeliveryZoneId",
                        column: x => x.DeliveryZoneId,
                        principalTable: "DeliveryZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderMenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceForUnit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderMenuItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderMenuItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderMenuItemId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_OrderMenuItems_MenuItemId",
                table: "OrderMenuItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMenuItems_OrderId",
                table: "OrderMenuItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Cash_FinishShift",
                table: "Orders",
                columns: new[] { "Cash", "FinishShift" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryZoneId",
                table: "Orders",
                column: "DeliveryZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EmployeeId",
                table: "Orders",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReceiverId",
                table: "Orders",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellerId",
                table: "Orders",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId");
        }
    }
}
