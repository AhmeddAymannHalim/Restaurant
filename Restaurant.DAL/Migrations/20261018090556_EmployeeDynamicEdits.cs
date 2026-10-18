using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeDynamicEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Administration",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "InsuranceAmount",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "InsurancePercent",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SalaryType",
                table: "Employees");

            migrationBuilder.AddColumn<decimal>(
                name: "AllowancesAmount",
                table: "InsuranceCategories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasicWage",
                table: "InsuranceCategories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompanyPercent",
                table: "InsuranceCategories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeePercent",
                table: "InsuranceCategories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinuteRate",
                table: "Employees",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowancesAmount",
                table: "InsuranceCategories");

            migrationBuilder.DropColumn(
                name: "BasicWage",
                table: "InsuranceCategories");

            migrationBuilder.DropColumn(
                name: "CompanyPercent",
                table: "InsuranceCategories");

            migrationBuilder.DropColumn(
                name: "EmployeePercent",
                table: "InsuranceCategories");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinuteRate",
                table: "Employees",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Administration",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceAmount",
                table: "Employees",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsurancePercent",
                table: "Employees",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Salary",
                table: "Employees",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalaryType",
                table: "Employees",
                type: "int",
                nullable: true);
        }
    }
}
