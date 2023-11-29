using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountIdToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Accounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"UPDATE Accounts
SET AccountId = 'A' + STR(ABS(CAST(CRYPT_GEN_RANDOM(4) AS INT))%(9999999-1000000)+1000000, 7, 0)
WHERE AccountId IS NULL or AccountId = ''");


            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId_AccountId",
                table: "Accounts",
                columns: new[] { "TenantId", "AccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_TenantId_AccountId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Accounts");
        }
    }
}
