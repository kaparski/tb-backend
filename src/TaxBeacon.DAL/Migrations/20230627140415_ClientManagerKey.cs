using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ClientManagerKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientManagers",
                table: "ClientManagers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientManagers",
                table: "ClientManagers",
                columns: new[] { "TenantId", "AccountId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientManagers",
                table: "ClientManagers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientManagers",
                table: "ClientManagers",
                columns: new[] { "TenantId", "AccountId" });
        }
    }
}
