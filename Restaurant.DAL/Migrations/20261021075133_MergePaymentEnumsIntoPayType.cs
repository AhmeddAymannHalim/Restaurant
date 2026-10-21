using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MergePaymentEnumsIntoPayType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PayType keeps only Cash (1) and Later (2) like the ERP: Installment (3) -> Later, CreditCard (4) -> Cash
            migrationBuilder.Sql("UPDATE Customers SET BillPayType = CASE BillPayType WHEN 3 THEN 2 WHEN 4 THEN 1 ELSE BillPayType END");
            migrationBuilder.Sql("UPDATE Suppliers SET BillPayType = CASE BillPayType WHEN 3 THEN 2 WHEN 4 THEN 1 ELSE BillPayType END");
            migrationBuilder.Sql("UPDATE BillMasters SET PayType = CASE PayType WHEN 3 THEN 2 WHEN 4 THEN 1 ELSE PayType END WHERE PayType IS NOT NULL");
            migrationBuilder.Sql("UPDATE BillSettings SET DefaultPayType = CASE DefaultPayType WHEN 3 THEN 2 WHEN 4 THEN 1 ELSE DefaultPayType END");

            migrationBuilder.DropColumn(
                name: "jopType",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "jopType",
                table: "Technicians",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Orders",
                type: "int",
                nullable: true);
        }
    }
}
