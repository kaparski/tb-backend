using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedDocumentsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentLength = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Documents_TenantUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalTable: "TenantUsers",
                        principalColumns: new[] { "TenantId", "UserId" });
                    table.ForeignKey(
                        name: "FK_Documents_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountDocuments",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountDocuments", x => new { x.TenantId, x.AccountId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_AccountDocuments_Accounts_TenantId_AccountId",
                        columns: x => new { x.TenantId, x.AccountId },
                        principalTable: "Accounts",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountDocuments_Documents_TenantId_DocumentId",
                        columns: x => new { x.TenantId, x.DocumentId },
                        principalTable: "Documents",
                        principalColumns: new[] { "TenantId", "Id" });
                    table.ForeignKey(
                        name: "FK_AccountDocuments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EntityDocuments",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDocuments", x => new { x.TenantId, x.EntityId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_EntityDocuments_Documents_TenantId_DocumentId",
                        columns: x => new { x.TenantId, x.DocumentId },
                        principalTable: "Documents",
                        principalColumns: new[] { "TenantId", "Id" });
                    table.ForeignKey(
                        name: "FK_EntityDocuments_Entities_TenantId_EntityId",
                        columns: x => new { x.TenantId, x.EntityId },
                        principalTable: "Entities",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityDocuments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocationDocuments",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationDocuments", x => new { x.TenantId, x.LocationId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_LocationDocuments_Documents_TenantId_DocumentId",
                        columns: x => new { x.TenantId, x.DocumentId },
                        principalTable: "Documents",
                        principalColumns: new[] { "TenantId", "Id" });
                    table.ForeignKey(
                        name: "FK_LocationDocuments_Locations_TenantId_LocationId",
                        columns: x => new { x.TenantId, x.LocationId },
                        principalTable: "Locations",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationDocuments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocuments_TenantId_DocumentId",
                table: "AccountDocuments",
                columns: new[] { "TenantId", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TenantId_UserId",
                table: "Documents",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityDocuments_TenantId_DocumentId",
                table: "EntityDocuments",
                columns: new[] { "TenantId", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationDocuments_TenantId_DocumentId",
                table: "LocationDocuments",
                columns: new[] { "TenantId", "DocumentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountDocuments");

            migrationBuilder.DropTable(
                name: "EntityDocuments");

            migrationBuilder.DropTable(
                name: "LocationDocuments");

            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
