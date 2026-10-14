using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BackfillEmployeePhones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Employees saved before EmployeePhones existed: their main mobile becomes the first phone row.
            migrationBuilder.Sql(@"
                INSERT INTO EmployeePhones (EmployeeId, PhoneNumber, CreatedAt)
                SELECT e.Id, LTRIM(RTRIM(e.PhoneNumber)), GETDATE()
                FROM Employees e
                WHERE e.PhoneNumber IS NOT NULL
                  AND LTRIM(RTRIM(e.PhoneNumber)) <> ''
                  AND NOT EXISTS (SELECT 1 FROM EmployeePhones p
                                  WHERE p.EmployeeId = e.Id AND p.PhoneNumber = LTRIM(RTRIM(e.PhoneNumber)));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
