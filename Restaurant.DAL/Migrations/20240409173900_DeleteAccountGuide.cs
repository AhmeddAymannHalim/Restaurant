using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Data.Migrations
{
    public partial class DeleteAccountGuide : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
