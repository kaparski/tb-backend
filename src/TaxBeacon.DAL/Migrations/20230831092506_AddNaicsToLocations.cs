using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNaicsToLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryNaicsCode",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_PrimaryNaicsCode",
                table: "Locations",
                column: "PrimaryNaicsCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_NaicsCodes_PrimaryNaicsCode",
                table: "Locations",
                column: "PrimaryNaicsCode",
                principalTable: "NaicsCodes",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_NaicsCodes_PrimaryNaicsCode",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_PrimaryNaicsCode",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "PrimaryNaicsCode",
                table: "Locations");
        }
    }
}
