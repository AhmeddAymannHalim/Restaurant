using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SaveOrderTaxPercentOnBill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE b SET b.BillExtraPercentage = ISNULL((SELECT TOP 1 s.OrderTax FROM ApplicationSettings s ORDER BY s.Id DESC), 0)
FROM BillMasters b
INNER JOIN Orders o ON o.Id = b.Id
WHERE o.Tax > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE b SET b.BillExtraPercentage = 0
FROM BillMasters b
INNER JOIN Orders o ON o.Id = b.Id");
        }
    }
}
