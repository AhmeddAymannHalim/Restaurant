using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateJournalIntoEntryMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The ERP journal (EntryMasters/EntryDetails) becomes the only journal. Entries of invoices and vouchers were already
            // mirrored there; anything that has no ERP entry yet (e.g. manual entries) is copied before the old tables go.
            migrationBuilder.Sql(@"SET QUOTED_IDENTIFIER ON;
-- entry settings the copied entries need (same names the code creates on first use)
INSERT INTO EntrySettings (ArName, EnName, ArAbbreviation, EnAbbreviation, AutoPosting, EntryType, CostCenterBelong, ShowTheLastBalance, CreatedAt)
SELECT v.ArName, v.EnName, v.ArName, v.EnName, 1, v.EntryType, 0, 0, GETDATE()
FROM (VALUES (N'صرف', N'Cash payment', 2), (N'قيد يومي', N'Journal', 3), (N'دفعات عملاء', N'Customer payment', 10), (N'دفعات موردين', N'Supplier payment', 11)) AS v (ArName, EnName, EntryType)
WHERE NOT EXISTS (SELECT 1 FROM EntrySettings s WHERE s.EntryType = v.EntryType);

CREATE TABLE #Missing (JournalEntryId int NOT NULL PRIMARY KEY, Tag nvarchar(100) NOT NULL, EntryType int NOT NULL);
INSERT INTO #Missing (JournalEntryId, Tag, EntryType)
SELECT je.Id,
       CASE je.SourceType WHEN 1 THEN N'PaymentVoucher' WHEN 2 THEN N'Manual' WHEN 3 THEN N'CustomerPaymentVoucher' ELSE N'SupplierPaymentVoucher' END
         + N':' + ISNULL(CAST(je.SourceId AS nvarchar(20)), N''),
       CASE je.SourceType WHEN 1 THEN 2 WHEN 2 THEN 3 WHEN 3 THEN 10 ELSE 11 END
FROM JournalEntries je
WHERE je.SourceType IN (1, 2, 3, 4)
  AND NOT EXISTS (SELECT 1 FROM EntryMasters e
                  WHERE e.CollectEntryCode = CASE je.SourceType WHEN 1 THEN N'PaymentVoucher' WHEN 2 THEN N'Manual' WHEN 3 THEN N'CustomerPaymentVoucher' ELSE N'SupplierPaymentVoucher' END
                                             + N':' + ISNULL(CAST(je.SourceId AS nvarchar(20)), N''));

INSERT INTO EntryMasters (Code, EntryDate, TotalBalance, CurrencyRate, CollectEntryCode, IsPosted, EntryRemarks, ForKest, Paid,
                          EntryCurrencyDifference, Disable, ChequeType, ChequePaperType, MoneyType, EntrySettingId, CreatedAt)
SELECT je.EntryNumber, je.Date, ISNULL((SELECT SUM(l.Debit) FROM JournalEntryLines l WHERE l.JournalEntryId = je.Id), 0), 1, m.Tag, 1,
       je.Description, 0, 0, 0, 0, 1, 1, 0,
       (SELECT TOP 1 s.Id FROM EntrySettings s WHERE s.EntryType = m.EntryType), GETDATE()
FROM #Missing m JOIN JournalEntries je ON je.Id = m.JournalEntryId;

INSERT INTO EntryDetails (EntryRowCode, Credit, Debit, EntryDetailsRemarks, AccountId, EntryMasterId, CreatedAt)
SELECT CAST(ROW_NUMBER() OVER (PARTITION BY l.JournalEntryId ORDER BY l.Id) AS smallint), l.Credit, l.Debit, l.Description, l.AccountId,
       (SELECT TOP 1 e.Id FROM EntryMasters e WHERE e.CollectEntryCode = m.Tag ORDER BY e.Id DESC), GETDATE()
FROM JournalEntryLines l JOIN #Missing m ON m.JournalEntryId = l.JournalEntryId;

DROP TABLE #Missing;");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "JournalEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EntryNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CreatedByUserId",
                table: "JournalEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_Date",
                table: "JournalEntries",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_SourceType_SourceId",
                table: "JournalEntries",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountId",
                table: "JournalEntryLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");
        }
    }
}
