using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameReservationGuestName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableReservations_AspNetUsers_CustomerId",
                table: "TableReservations");

            migrationBuilder.DropIndex(
                name: "IX_TableReservations_CustomerId",
                table: "TableReservations");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "TableReservations");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "TableReservations",
                newName: "GuestName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GuestName",
                table: "TableReservations",
                newName: "CustomerName");

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "TableReservations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableReservations_CustomerId",
                table: "TableReservations",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableReservations_AspNetUsers_CustomerId",
                table: "TableReservations",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
