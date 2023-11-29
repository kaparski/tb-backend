using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedEntityPrimaryNaicsCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryNaicsCode",
                table: "Entities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entities_PrimaryNaicsCode",
                table: "Entities",
                column: "PrimaryNaicsCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_NaicsCodes_PrimaryNaicsCode",
                table: "Entities",
                column: "PrimaryNaicsCode",
                principalTable: "NaicsCodes",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entities_NaicsCodes_PrimaryNaicsCode",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_Entities_PrimaryNaicsCode",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "PrimaryNaicsCode",
                table: "Entities");
        }
    }
}
