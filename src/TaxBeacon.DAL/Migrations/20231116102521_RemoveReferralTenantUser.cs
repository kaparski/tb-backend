using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReferralTenantUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_TenantUsers_TenantUserTenantId_TenantUserUserId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_TenantUserTenantId_TenantUserUserId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "TenantUserTenantId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "TenantUserUserId",
                table: "Referrals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantUserTenantId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantUserUserId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_TenantUserTenantId_TenantUserUserId",
                table: "Referrals",
                columns: new[] { "TenantUserTenantId", "TenantUserUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_TenantUsers_TenantUserTenantId_TenantUserUserId",
                table: "Referrals",
                columns: new[] { "TenantUserTenantId", "TenantUserUserId" },
                principalTable: "TenantUsers",
                principalColumns: new[] { "TenantId", "UserId" });
        }
    }
}
