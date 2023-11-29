using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsNaicsCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryNaicsCode",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PrimaryNaicsCode",
                table: "Accounts",
                column: "PrimaryNaicsCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_NaicsCodes_PrimaryNaicsCode",
                table: "Accounts",
                column: "PrimaryNaicsCode",
                principalTable: "NaicsCodes",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_NaicsCodes_PrimaryNaicsCode",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_PrimaryNaicsCode",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PrimaryNaicsCode",
                table: "Accounts");
        }
    }
}
