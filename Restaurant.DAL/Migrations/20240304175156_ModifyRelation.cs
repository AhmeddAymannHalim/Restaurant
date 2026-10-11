using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Data.Migrations
{
    public partial class ModifyRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Kitchens_kitchenId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_kitchenId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "kitchenId",
                table: "Branches");

            migrationBuilder.AddColumn<int>(
                name: "kitchenId",
                table: "MenuSections",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuSections_kitchenId",
                table: "MenuSections",
                column: "kitchenId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuSections_Kitchens_kitchenId",
                table: "MenuSections",
                column: "kitchenId",
                principalTable: "Kitchens",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuSections_Kitchens_kitchenId",
                table: "MenuSections");

            migrationBuilder.DropIndex(
                name: "IX_MenuSections_kitchenId",
                table: "MenuSections");

            migrationBuilder.DropColumn(
                name: "kitchenId",
                table: "MenuSections");

            migrationBuilder.AddColumn<int>(
                name: "kitchenId",
                table: "Branches",
                type: "int",
                nullable: true);

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
    }
}
