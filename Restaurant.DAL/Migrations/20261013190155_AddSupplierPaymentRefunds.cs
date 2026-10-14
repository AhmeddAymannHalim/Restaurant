using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPaymentRefunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OriginalPaymentId",
                table: "PaymentRefunds",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SupplierPaymentId",
                table: "PaymentRefunds",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_SupplierPaymentId",
                table: "PaymentRefunds",
                column: "SupplierPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRefunds_PaymentInvoiceSupplier_SupplierPaymentId",
                table: "PaymentRefunds",
                column: "SupplierPaymentId",
                principalTable: "PaymentInvoiceSupplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRefunds_PaymentInvoiceSupplier_SupplierPaymentId",
                table: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_SupplierPaymentId",
                table: "PaymentRefunds");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentId",
                table: "PaymentRefunds");

            migrationBuilder.AlterColumn<int>(
                name: "OriginalPaymentId",
                table: "PaymentRefunds",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
