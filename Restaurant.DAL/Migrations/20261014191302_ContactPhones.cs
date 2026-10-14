using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ContactPhones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerPhones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPhones_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorePhones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorePhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorePhones_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPhones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPhones_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPhones_CustomerId",
                table: "CustomerPhones",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_StorePhones_StoreId",
                table: "StorePhones",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPhones_SupplierId",
                table: "SupplierPhones",
                column: "SupplierId");

            migrationBuilder.Sql(@"
INSERT INTO CustomerPhones (PhoneNumber, CustomerId, CreatedAt)
SELECT LTRIM(RTRIM(PhoneNumber2)), Id, GETDATE() FROM Customers
WHERE PhoneNumber2 IS NOT NULL AND LTRIM(RTRIM(PhoneNumber2)) <> '' AND ISNULL(PhoneNumber1, '') <> LTRIM(RTRIM(PhoneNumber2));
INSERT INTO SupplierPhones (PhoneNumber, SupplierId, CreatedAt)
SELECT LTRIM(RTRIM(PhoneNumber2)), Id, GETDATE() FROM Suppliers
WHERE PhoneNumber2 IS NOT NULL AND LTRIM(RTRIM(PhoneNumber2)) <> '' AND ISNULL(PhoneNumber1, '') <> LTRIM(RTRIM(PhoneNumber2));
INSERT INTO StorePhones (PhoneNumber, StoreId, CreatedAt)
SELECT LTRIM(RTRIM(PhoneNumber2)), Id, GETDATE() FROM Stores
WHERE PhoneNumber2 IS NOT NULL AND LTRIM(RTRIM(PhoneNumber2)) <> '' AND ISNULL(PhoneNumber1, '') <> LTRIM(RTRIM(PhoneNumber2));
INSERT INTO StorePhones (PhoneNumber, StoreId, CreatedAt)
SELECT LTRIM(RTRIM(PhoneNumber3)), Id, GETDATE() FROM Stores
WHERE PhoneNumber3 IS NOT NULL AND LTRIM(RTRIM(PhoneNumber3)) <> '' AND ISNULL(PhoneNumber1, '') <> LTRIM(RTRIM(PhoneNumber3))
  AND ISNULL(PhoneNumber2, '') <> LTRIM(RTRIM(PhoneNumber3));
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerPhones");

            migrationBuilder.DropTable(
                name: "StorePhones");

            migrationBuilder.DropTable(
                name: "SupplierPhones");
        }
    }
}
