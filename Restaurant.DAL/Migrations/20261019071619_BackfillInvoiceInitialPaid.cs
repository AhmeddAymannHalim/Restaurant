using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BackfillInvoiceInitialPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // bills saved before InitialPaidAmount existed carry their down payment only in AmountPaided
            migrationBuilder.Sql(@"
UPDATE b
SET b.InitialPaidAmount = b.AmountPaided
    - ISNULL((SELECT SUM(p.Paid) FROM PaymentInvoiceSupplier p WHERE p.BillMasterId = b.Id), 0)
    - ISNULL((SELECT SUM(p.Paid) FROM PaymentInvoiceCustomer p WHERE p.BillMasterId = b.Id), 0)
FROM BillMasters b
WHERE b.InitialPaidAmount = 0 AND b.AmountPaided > 0");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
