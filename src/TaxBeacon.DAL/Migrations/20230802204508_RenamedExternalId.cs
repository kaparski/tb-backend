using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenamedExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AadB2CObjectId",
                table: "Users",
                newName: "IdpExternalId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_AadB2CObjectId",
                table: "Users",
                newName: "IX_Users_IdpExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdpExternalId",
                table: "Users",
                newName: "AadB2CObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_IdpExternalId",
                table: "Users",
                newName: "IX_Users_AadB2CObjectId");
        }
    }
}
