using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenamedColumnsInEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserStatus",
                table: "Users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "LastModifiedUtc",
                table: "Users",
                newName: "LastModifiedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "LastLoginDateUtc",
                table: "Users",
                newName: "LastLoginDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUtc",
                table: "Users",
                newName: "DeletedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUtc",
                table: "Users",
                newName: "CreatedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedUtc",
                table: "Tenants",
                newName: "LastModifiedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUtc",
                table: "Tenants",
                newName: "DeletedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUtc",
                table: "Tenants",
                newName: "CreatedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedUtc",
                table: "Roles",
                newName: "LastModifiedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUtc",
                table: "Roles",
                newName: "DeletedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUtc",
                table: "Roles",
                newName: "CreatedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedUtc",
                table: "Permissions",
                newName: "LastModifiedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUtc",
                table: "Permissions",
                newName: "DeletedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUtc",
                table: "Permissions",
                newName: "CreatedDateTimeUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Users",
                newName: "UserStatus");

            migrationBuilder.RenameColumn(
                name: "LastModifiedDateTimeUtc",
                table: "Users",
                newName: "LastModifiedUtc");

            migrationBuilder.RenameColumn(
                name: "LastLoginDateTimeUtc",
                table: "Users",
                newName: "LastLoginDateUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateTimeUtc",
                table: "Users",
                newName: "DeletedDateUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateTimeUtc",
                table: "Users",
                newName: "CreatedDateUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedDateTimeUtc",
                table: "Tenants",
                newName: "LastModifiedUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateTimeUtc",
                table: "Tenants",
                newName: "DeletedDateUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateTimeUtc",
                table: "Tenants",
                newName: "CreatedDateUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedDateTimeUtc",
                table: "Roles",
                newName: "LastModifiedUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateTimeUtc",
                table: "Roles",
                newName: "DeletedDateUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateTimeUtc",
                table: "Roles",
                newName: "CreatedDateUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedDateTimeUtc",
                table: "Permissions",
                newName: "LastModifiedUtc");

            migrationBuilder.RenameColumn(
                name: "DeletedDateTimeUtc",
                table: "Permissions",
                newName: "DeletedDateUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedDateTimeUtc",
                table: "Permissions",
                newName: "CreatedDateUtc");
        }
    }
}
