using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Data.Migrations
{
    public partial class AddingAccountGuide : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    PayTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayTypeArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayTypeEnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayTypeNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayTypes", x => x.PayTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SellTypes",
                columns: table => new
                {
                    SellTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellTypeArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SellTypeEnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowOrNot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SellDefault = table.Column<bool>(type: "bit", nullable: true),
                    SellPlusPrice = table.Column<double>(type: "float", nullable: true)
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
                    AccCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccArName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccEnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    AccDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccCheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccStateId = table.Column<byte>(type: "tinyint", nullable: false),
                    AccCredit = table.Column<double>(type: "float", nullable: false),
                    AccDebit = table.Column<double>(type: "float", nullable: false),
                    AccMaxDebit = table.Column<double>(type: "float", nullable: true),
                    CurrencyId = table.Column<short>(type: "smallint", nullable: false),
                    ParentAccId = table.Column<int>(type: "int", nullable: true),
                    FinalAccId = table.Column<int>(type: "int", nullable: true),
                    AccLevel = table.Column<int>(type: "int", nullable: true),
                    AccRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccNsons = table.Column<short>(type: "smallint", nullable: false),
                    AccTurnningYesOrNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccDebitOrCreditOrWithout = table.Column<byte>(type: "tinyint", nullable: false),
                    AccCustomerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class = table.Column<bool>(type: "bit", nullable: true),
                    AccMustCostCenter = table.Column<bool>(type: "bit", nullable: true)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "PayTypes");

            migrationBuilder.DropTable(
                name: "SellTypes");

            migrationBuilder.DropTable(
                name: "AccountStates");

            migrationBuilder.DropTable(
                name: "AccountTypes");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
