using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Data.Migrations
{
    public partial class addSApplcationSettingAndPrinter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "NumberOfCustomerPrint",
                table: "ApplicationSettings",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "NumberOfKitchenPrint",
                table: "ApplicationSettings",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderDiscount",
                table: "ApplicationSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderService",
                table: "ApplicationSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PrintAfterOrder",
                table: "ApplicationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrintForTakeawayOrder",
                table: "ApplicationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTotalShift",
                table: "ApplicationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                name: "PrinterSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrinterForAllSections = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrinterForCashier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrinterForEachSection = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrinterForSections_SectionId",
                table: "PrinterForSections",
                column: "SectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrinterForSections");

            migrationBuilder.DropTable(
                name: "PrinterSettings");

            migrationBuilder.DropColumn(
                name: "NumberOfCustomerPrint",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "NumberOfKitchenPrint",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "OrderDiscount",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "OrderService",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "PrintAfterOrder",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "PrintForTakeawayOrder",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "ShowTotalShift",
                table: "ApplicationSettings");
        }
    }
}
