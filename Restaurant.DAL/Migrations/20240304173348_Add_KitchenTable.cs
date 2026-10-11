using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Data.Migrations
{
    public partial class Add_KitchenTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "kitchenId",
                table: "Branches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kitchens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kitchens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_kitchenId",
                table: "Branches",
                column: "kitchenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Kitchens_kitchenId",
                table: "Branches",
                column: "kitchenId",
                principalTable: "Kitchens",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Kitchens_kitchenId",
                table: "Branches");

            migrationBuilder.DropTable(
                name: "Kitchens");

            migrationBuilder.DropIndex(
                name: "IX_Branches_kitchenId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "kitchenId",
                table: "Branches");
        }
    }
}
