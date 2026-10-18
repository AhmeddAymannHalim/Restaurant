using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class LinkEmployeeRowsToTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeductionTypeId",
                table: "EmployeeFixedDeductions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AllowanceTypeId",
                table: "EmployeeAllowances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeFixedDeductions_DeductionTypeId",
                table: "EmployeeFixedDeductions",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAllowances_AllowanceTypeId",
                table: "EmployeeAllowances",
                column: "AllowanceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAllowances_AllowanceTypes_AllowanceTypeId",
                table: "EmployeeAllowances",
                column: "AllowanceTypeId",
                principalTable: "AllowanceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeFixedDeductions_DeductionTypes_DeductionTypeId",
                table: "EmployeeFixedDeductions",
                column: "DeductionTypeId",
                principalTable: "DeductionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAllowances_AllowanceTypes_AllowanceTypeId",
                table: "EmployeeAllowances");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeFixedDeductions_DeductionTypes_DeductionTypeId",
                table: "EmployeeFixedDeductions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeFixedDeductions_DeductionTypeId",
                table: "EmployeeFixedDeductions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAllowances_AllowanceTypeId",
                table: "EmployeeAllowances");

            migrationBuilder.DropColumn(
                name: "DeductionTypeId",
                table: "EmployeeFixedDeductions");

            migrationBuilder.DropColumn(
                name: "AllowanceTypeId",
                table: "EmployeeAllowances");
        }
    }
}
