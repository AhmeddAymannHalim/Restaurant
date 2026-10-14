using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EmployeePhones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeePhones_EmployeeId",
                table: "EmployeePhones");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "EmployeePhones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePhones_EmployeeId_PhoneNumber",
                table: "EmployeePhones",
                columns: new[] { "EmployeeId", "PhoneNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeePhones_EmployeeId_PhoneNumber",
                table: "EmployeePhones");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "EmployeePhones",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePhones_EmployeeId",
                table: "EmployeePhones",
                column: "EmployeeId");
        }
    }
}
