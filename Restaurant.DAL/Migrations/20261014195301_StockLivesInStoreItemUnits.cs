using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class StockLivesInStoreItemUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreConvertProducts_StoreProducts_StoreProductId",
                table: "StoreConvertProducts");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "StoreItemUnits",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);


            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Units WHERE Code = N'BASE')
    INSERT INTO Units (Code, ArName, EnName, CreatedAt) VALUES (N'BASE', N'وحدة', N'Unit', GETDATE());

INSERT INTO Items (Code, ArName, EnName, CreatedAt, HasProduction, HasExpiry, HasSerialInput, HasSerialOutput, HasVat, StopTrading,
                   WeightInGrams, TaxPercentage, DiscountPercentage, WeightedAverageQuantityOnHand, HasWeightedAverageHistory,
                   RequiresCostingRebuild, RequiresAverageRecalculation, EtaItemType)
SELECT N'P' + CONVERT(nvarchar(20), p.Id), p.ArName, p.EnName, GETDATE(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
FROM Products p
WHERE NOT EXISTS (SELECT 1 FROM Items i WHERE i.Code = N'P' + CONVERT(nvarchar(20), p.Id));

DECLARE @base int = (SELECT TOP 1 Id FROM Units WHERE Code = N'BASE');

INSERT INTO ItemUnits (ItemId, UnitId, Factor, CreatedAt)
SELECT DISTINCT i.Id, @base, 1, GETDATE()
FROM StoreProducts sp
JOIN Items i ON i.Code = N'P' + CONVERT(nvarchar(20), sp.ProductId)
WHERE NOT EXISTS (SELECT 1 FROM ItemUnits iu WHERE iu.ItemId = i.Id AND iu.UnitId = @base);

DELETE siu FROM StoreItemUnits siu
JOIN Items i ON i.Id = siu.ItemId
WHERE i.Code LIKE N'P%' AND EXISTS (SELECT 1 FROM Products p WHERE N'P' + CONVERT(nvarchar(20), p.Id) = i.Code);

INSERT INTO StoreItemUnits (StoreId, ItemId, UnitId, Quantity, CreatedAt)
SELECT sp.StoreId, i.Id, @base, sp.Quantity, GETDATE()
FROM StoreProducts sp
JOIN Items i ON i.Code = N'P' + CONVERT(nvarchar(20), sp.ProductId);

UPDATE c SET c.StoreProductId = sp.ProductId
FROM StoreConvertProducts c
JOIN StoreProducts sp ON sp.Id = c.StoreProductId;
");

            migrationBuilder.DropTable(
                name: "StoreProducts");

            migrationBuilder.RenameColumn(
                name: "StoreProductId",
                table: "StoreConvertProducts",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_StoreConvertProducts_StoreProductId",
                table: "StoreConvertProducts",
                newName: "IX_StoreConvertProducts_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreConvertProducts_Products_ProductId",
                table: "StoreConvertProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreConvertProducts_Products_ProductId",
                table: "StoreConvertProducts");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "StoreConvertProducts",
                newName: "StoreProductId");

            migrationBuilder.RenameIndex(
                name: "IX_StoreConvertProducts_ProductId",
                table: "StoreConvertProducts",
                newName: "IX_StoreConvertProducts_StoreProductId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "StoreItemUnits",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.CreateTable(
                name: "StoreProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreProducts_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreProducts_ProductId",
                table: "StoreProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreProducts_StoreId_ProductId",
                table: "StoreProducts",
                columns: new[] { "StoreId", "ProductId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreConvertProducts_StoreProducts_StoreProductId",
                table: "StoreConvertProducts",
                column: "StoreProductId",
                principalTable: "StoreProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
