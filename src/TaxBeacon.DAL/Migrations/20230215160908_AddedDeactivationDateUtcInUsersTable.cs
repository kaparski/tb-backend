using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeactivationDateUtcInUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivationDateUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(201)",
                maxLength: 201,
                nullable: false,
                computedColumnSql: "TRIM(CONCAT([FirstName], ' ', [LastName]))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(202)",
                oldMaxLength: 202,
                oldComputedColumnSql: "TRIM(CONCAT([FirstName], ' ', [LastName]))",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivationDateUtc",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(202)",
                maxLength: 202,
                nullable: false,
                computedColumnSql: "TRIM(CONCAT([FirstName], ' ', [LastName]))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(201)",
                oldMaxLength: 201,
                oldComputedColumnSql: "TRIM(CONCAT([FirstName], ' ', [LastName]))",
                oldStored: true);
        }
    }
}
