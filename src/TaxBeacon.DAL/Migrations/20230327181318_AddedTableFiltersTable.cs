using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedTableFiltersTable : Migration
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

            migrationBuilder.CreateTable(
                name: "TableFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TableType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableFilters_Tenants_UserId",
                        column: x => x.UserId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableFilters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableFilters_TenantId_TableType_UserId",
                table: "TableFilters",
                columns: new[] { "TenantId", "TableType", "UserId" })
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_TableFilters_UserId",
                table: "TableFilters",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableFilters");

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
