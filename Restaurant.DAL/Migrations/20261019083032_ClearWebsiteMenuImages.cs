using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ClearWebsiteMenuImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // dishes that pointed to the old website pictures (deleted) show no picture instead of a broken one
            migrationBuilder.Sql("UPDATE MenuItems SET ImageUrl = NULL WHERE ImageUrl LIKE '/Images/%' OR ImageUrl LIKE '/assets/%'");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
