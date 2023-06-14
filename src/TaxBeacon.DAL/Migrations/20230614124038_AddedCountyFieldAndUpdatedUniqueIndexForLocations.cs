using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedCountyFieldAndUpdatedUniqueIndexForLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_AccountId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_TenantId_LocationId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_TenantId_Name",
                table: "Locations");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Locations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_AccountId_LocationId_Name",
                table: "Locations",
                columns: new[] { "AccountId", "LocationId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_AccountId_LocationId_Name",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Locations");

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
        }
    }
}
