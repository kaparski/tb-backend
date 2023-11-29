using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakeLocationNameUniqueWithinAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_TenantId_Name",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_AccountId_Name",
                table: "Locations",
                columns: new[] { "AccountId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_AccountId_Name",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_TenantId_Name",
                table: "Locations",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }
    }
}
