using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class LinkPaymentsToInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillMasterId",
                table: "PaymentInvoiceSupplier",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillMasterId",
                table: "PaymentInvoiceCustomer",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceSupplier_BillMasterId",
                table: "PaymentInvoiceSupplier",
                column: "BillMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceCustomer_BillMasterId",
                table: "PaymentInvoiceCustomer",
                column: "BillMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInvoiceCustomer_BillMasters_BillMasterId",
                table: "PaymentInvoiceCustomer",
                column: "BillMasterId",
                principalTable: "BillMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInvoiceSupplier_BillMasters_BillMasterId",
                table: "PaymentInvoiceSupplier",
                column: "BillMasterId",
                principalTable: "BillMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInvoiceCustomer_BillMasters_BillMasterId",
                table: "PaymentInvoiceCustomer");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInvoiceSupplier_BillMasters_BillMasterId",
                table: "PaymentInvoiceSupplier");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInvoiceSupplier_BillMasterId",
                table: "PaymentInvoiceSupplier");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInvoiceCustomer_BillMasterId",
                table: "PaymentInvoiceCustomer");

            migrationBuilder.DropColumn(
                name: "BillMasterId",
                table: "PaymentInvoiceSupplier");

            migrationBuilder.DropColumn(
                name: "BillMasterId",
                table: "PaymentInvoiceCustomer");
        }
    }
}
