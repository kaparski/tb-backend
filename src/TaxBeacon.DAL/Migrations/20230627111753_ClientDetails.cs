using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ClientDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Users_ManagerId",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Clients",
                newName: "PrimaryContactId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_ManagerId",
                table: "Clients",
                newName: "IX_Clients_PrimaryContactId");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualRevenue",
                table: "Clients",
                type: "decimal(15,2)",
                precision: 15,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivationDateTimeUtc",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoundationYear",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivationDateTimeUtc",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientManagers",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientManagers", x => new { x.TenantId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_ClientManagers_Clients_TenantId_AccountId",
                        columns: x => new { x.TenantId, x.AccountId },
                        principalTable: "Clients",
                        principalColumns: new[] { "TenantId", "AccountId" });
                    table.ForeignKey(
                        name: "FK_ClientManagers_TenantUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalTable: "TenantUsers",
                        principalColumns: new[] { "TenantId", "UserId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientManagers_TenantId_UserId",
                table: "ClientManagers",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Contacts_PrimaryContactId",
                table: "Clients",
                column: "PrimaryContactId",
                principalTable: "Contacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Contacts_PrimaryContactId",
                table: "Clients");

            migrationBuilder.DropTable(
                name: "ClientManagers");

            migrationBuilder.DropColumn(
                name: "AnnualRevenue",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DeactivationDateTimeUtc",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FoundationYear",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ReactivationDateTimeUtc",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "PrimaryContactId",
                table: "Clients",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_PrimaryContactId",
                table: "Clients",
                newName: "IX_Clients_ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Users_ManagerId",
                table: "Clients",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
