using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_AccountId_LocationId_Name",
                table: "Locations");

            migrationBuilder.CreateTable(
                name: "LocationActivityLogs",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationActivityLogs", x => new { x.TenantId, x.LocationId, x.Date });
                    table.ForeignKey(
                        name: "FK_LocationActivityLogs_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationActivityLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_AccountId",
                table: "Locations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_TenantId_LocationId",
                table: "Locations",
                columns: new[] { "TenantId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_TenantId_Name",
                table: "Locations",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationActivityLogs_LocationId",
                table: "LocationActivityLogs",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_Locations_AccountId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_TenantId_LocationId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_TenantId_Name",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_AccountId_LocationId_Name",
                table: "Locations",
                columns: new[] { "AccountId", "LocationId", "Name" },
                unique: true);
        }
    }
}
