using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameBranchToCompanyBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillMasters_Branches_BranchId",
                table: "BillMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Branches_BranchId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Branches_BranchId",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Branches_BranchId",
                table: "Suppliers");


            migrationBuilder.RenameColumn(
                name: "BranchId",
                table: "Customers",
                newName: "CompanyBranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_BranchId",
                table: "Customers",
                newName: "IX_Customers_CompanyBranchId");

            migrationBuilder.RenameColumn(
                name: "BranchId",
                table: "BillMasters",
                newName: "CompanyBranchId");

            migrationBuilder.RenameIndex(
                name: "IX_BillMasters_BranchId",
                table: "BillMasters",
                newName: "IX_BillMasters_CompanyBranchId");

            migrationBuilder.CreateTable(
                name: "CompanyBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ArName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EnName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArVillage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnVillage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArStreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnStreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EnAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    buildingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber3 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    GovId = table.Column<int>(type: "int", nullable: true),
                    TownId = table.Column<int>(type: "int", nullable: true),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    State = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_CompanyBranches_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CompanyBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Governorates_GovId",
                        column: x => x.GovId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Towns_TownId",
                        column: x => x.TownId,
                        principalTable: "Towns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_ArName",
                table: "CompanyBranches",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_CompanyId",
                table: "CompanyBranches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_CostCenterId",
                table: "CompanyBranches",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_CountryId",
                table: "CompanyBranches",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_CurrencyId",
                table: "CompanyBranches",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_EnName",
                table: "CompanyBranches",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_GovId",
                table: "CompanyBranches",
                column: "GovId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_ParentId",
                table: "CompanyBranches",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_TaxId",
                table: "CompanyBranches",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_TownId",
                table: "CompanyBranches",
                column: "TownId");

            // keep the rows: copy them to the renamed table, then drop the old one
            migrationBuilder.Sql("SET IDENTITY_INSERT [CompanyBranches] ON; INSERT INTO [CompanyBranches] ([Id], [Code], [ArName], [EnName], [VatNumber], [RegistrationNumber], [Email], [PostalCode], [ImageUrl], [ArVillage], [EnVillage], [ArStreetName], [EnStreetName], [ArAddress], [EnAddress], [buildingNo], [PhoneNumber1], [PhoneNumber2], [PhoneNumber3], [Note], [CompanyId], [ParentId], [CountryId], [GovId], [TownId], [TaxId], [CurrencyId], [CostCenterId], [State], [CreatedAt], [UpdatedAt], [DeletedAt], [CreatedById], [UpdatedById], [DeletedById]) SELECT [Id], [Code], [ArName], [EnName], [VatNumber], [RegistrationNumber], [Email], [PostalCode], [ImageUrl], [ArVillage], [EnVillage], [ArStreetName], [EnStreetName], [ArAddress], [EnAddress], [buildingNo], [PhoneNumber1], [PhoneNumber2], [PhoneNumber3], [Note], [CompanyId], [ParentId], [CountryId], [GovId], [TownId], [TaxId], [CurrencyId], [CostCenterId], [State], [CreatedAt], [UpdatedAt], [DeletedAt], [CreatedById], [UpdatedById], [DeletedById] FROM [Branches]; SET IDENTITY_INSERT [CompanyBranches] OFF;");
            migrationBuilder.DropTable(
                name: "Branches");
            migrationBuilder.AddForeignKey(
                name: "FK_BillMasters_CompanyBranches_CompanyBranchId",
                table: "BillMasters",
                column: "CompanyBranchId",
                principalTable: "CompanyBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_CompanyBranches_CompanyBranchId",
                table: "Customers",
                column: "CompanyBranchId",
                principalTable: "CompanyBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_CompanyBranches_BranchId",
                table: "Employees",
                column: "BranchId",
                principalTable: "CompanyBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_CompanyBranches_BranchId",
                table: "Stores",
                column: "BranchId",
                principalTable: "CompanyBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_CompanyBranches_BranchId",
                table: "Suppliers",
                column: "BranchId",
                principalTable: "CompanyBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillMasters_CompanyBranches_CompanyBranchId",
                table: "BillMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_CompanyBranches_CompanyBranchId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_CompanyBranches_BranchId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_CompanyBranches_BranchId",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_CompanyBranches_BranchId",
                table: "Suppliers");


            migrationBuilder.RenameColumn(
                name: "CompanyBranchId",
                table: "Customers",
                newName: "BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_CompanyBranchId",
                table: "Customers",
                newName: "IX_Customers_BranchId");

            migrationBuilder.RenameColumn(
                name: "CompanyBranchId",
                table: "BillMasters",
                newName: "BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_BillMasters_CompanyBranchId",
                table: "BillMasters",
                newName: "IX_BillMasters_BranchId");

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    GovId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TownId = table.Column<int>(type: "int", nullable: true),
                    ArAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ArName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ArStreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArVillage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EnName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EnStreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnVillage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber3 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    buildingNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Branches_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Governorates_GovId",
                        column: x => x.GovId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Towns_TownId",
                        column: x => x.TownId,
                        principalTable: "Towns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ArName",
                table: "Branches",
                column: "ArName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId",
                table: "Branches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CostCenterId",
                table: "Branches",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CountryId",
                table: "Branches",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CurrencyId",
                table: "Branches",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_EnName",
                table: "Branches",
                column: "EnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_GovId",
                table: "Branches",
                column: "GovId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ParentId",
                table: "Branches",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TaxId",
                table: "Branches",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TownId",
                table: "Branches",
                column: "TownId");

            // keep the rows: copy them to the renamed table, then drop the old one
            migrationBuilder.Sql("SET IDENTITY_INSERT [Branches] ON; INSERT INTO [Branches] ([Id], [Code], [ArName], [EnName], [VatNumber], [RegistrationNumber], [Email], [PostalCode], [ImageUrl], [ArVillage], [EnVillage], [ArStreetName], [EnStreetName], [ArAddress], [EnAddress], [buildingNo], [PhoneNumber1], [PhoneNumber2], [PhoneNumber3], [Note], [CompanyId], [ParentId], [CountryId], [GovId], [TownId], [TaxId], [CurrencyId], [CostCenterId], [State], [CreatedAt], [UpdatedAt], [DeletedAt], [CreatedById], [UpdatedById], [DeletedById]) SELECT [Id], [Code], [ArName], [EnName], [VatNumber], [RegistrationNumber], [Email], [PostalCode], [ImageUrl], [ArVillage], [EnVillage], [ArStreetName], [EnStreetName], [ArAddress], [EnAddress], [buildingNo], [PhoneNumber1], [PhoneNumber2], [PhoneNumber3], [Note], [CompanyId], [ParentId], [CountryId], [GovId], [TownId], [TaxId], [CurrencyId], [CostCenterId], [State], [CreatedAt], [UpdatedAt], [DeletedAt], [CreatedById], [UpdatedById], [DeletedById] FROM [CompanyBranches]; SET IDENTITY_INSERT [Branches] OFF;");
            migrationBuilder.DropTable(
                name: "CompanyBranches");
            migrationBuilder.AddForeignKey(
                name: "FK_BillMasters_Branches_BranchId",
                table: "BillMasters",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Branches_BranchId",
                table: "Customers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Branches_BranchId",
                table: "Stores",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Branches_BranchId",
                table: "Suppliers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
