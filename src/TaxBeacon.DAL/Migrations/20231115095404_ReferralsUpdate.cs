using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ReferralsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_TenantUsers_TenantId_ManagerId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_TenantId_ManagerId",
                table: "Referrals");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Referrals",
                newName: "TenantUserUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationDateTimeUtc",
                table: "Referrals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivationDateTimeUtc",
                table: "Referrals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationType",
                table: "Referrals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryContactId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivationDateTimeUtc",
                table: "Referrals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantUserTenantId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Referrals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysOpen",
                table: "Referrals",
                type: "int",
                nullable: false,
                computedColumnSql: "DATEDIFF(second, COALESCE(ActivationDateTimeUtc, CreatedDateTimeUtc), COALESCE(DeactivationDateTimeUtc, GETUTCDATE())) / 86400");

            migrationBuilder.CreateTable(
                name: "ReferralManagers",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralManagers", x => new { x.TenantId, x.AccountId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ReferralManagers_Referrals_TenantId_AccountId",
                        columns: x => new { x.TenantId, x.AccountId },
                        principalTable: "Referrals",
                        principalColumns: new[] { "TenantId", "AccountId" });
                    table.ForeignKey(
                        name: "FK_ReferralManagers_TenantUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalTable: "TenantUsers",
                        principalColumns: new[] { "TenantId", "UserId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_TenantId_PrimaryContactId",
                table: "Referrals",
                columns: new[] { "TenantId", "PrimaryContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_TenantUserTenantId_TenantUserUserId",
                table: "Referrals",
                columns: new[] { "TenantUserTenantId", "TenantUserUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralManagers_TenantId_UserId",
                table: "ReferralManagers",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Contacts_TenantId_PrimaryContactId",
                table: "Referrals",
                columns: new[] { "TenantId", "PrimaryContactId" },
                principalTable: "Contacts",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_TenantUsers_TenantUserTenantId_TenantUserUserId",
                table: "Referrals",
                columns: new[] { "TenantUserTenantId", "TenantUserUserId" },
                principalTable: "TenantUsers",
                principalColumns: new[] { "TenantId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Contacts_TenantId_PrimaryContactId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_TenantUsers_TenantUserTenantId_TenantUserUserId",
                table: "Referrals");

            migrationBuilder.DropTable(
                name: "ReferralManagers");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_TenantId_PrimaryContactId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_TenantUserTenantId_TenantUserUserId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "DaysOpen",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ActivationDateTimeUtc",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "DeactivationDateTimeUtc",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "OrganizationType",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "PrimaryContactId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReactivationDateTimeUtc",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "TenantUserTenantId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Referrals");

            migrationBuilder.RenameColumn(
                name: "TenantUserUserId",
                table: "Referrals",
                newName: "ManagerId");

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
    }
}
