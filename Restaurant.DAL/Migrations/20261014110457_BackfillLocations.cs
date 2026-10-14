using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BackfillLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rows saved before the governorate and country were derived from the city.
            foreach (var table in new[] { "Customers", "Suppliers", "Employees", "Stores" })
            {
                migrationBuilder.Sql($@"
                    SET QUOTED_IDENTIFIER ON;
                    UPDATE x SET x.GovId = t.GovernorateId, x.CountryId = g.CountryId
                    FROM {table} x
                    JOIN Towns t ON t.Id = x.TownId
                    JOIN Governorates g ON g.Id = t.GovernorateId
                    WHERE x.GovId IS NULL OR x.CountryId IS NULL;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
