using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MoveReferralToTenantUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Users_ManagerId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_ManagerId",
                table: "Referrals");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_TenantId_ManagerId",
                table: "Referrals",
                columns: new[] { "TenantId", "ManagerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_TenantUsers_TenantId_ManagerId",
                table: "Referrals",
                columns: new[] { "TenantId", "ManagerId" },
                principalTable: "TenantUsers",
                principalColumns: new[] { "TenantId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_TenantUsers_TenantId_ManagerId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_TenantId_ManagerId",
                table: "Referrals");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ManagerId",
                table: "Referrals",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Users_ManagerId",
                table: "Referrals",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
