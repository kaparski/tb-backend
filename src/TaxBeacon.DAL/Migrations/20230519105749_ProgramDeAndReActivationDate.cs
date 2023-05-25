using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ProgramDeAndReActivationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivationDateTimeUtc",
                table: "TenantsPrograms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivationDateTimeUtc",
                table: "TenantsPrograms",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivationDateTimeUtc",
                table: "TenantsPrograms");

            migrationBuilder.DropColumn(
                name: "ReactivationDateTimeUtc",
                table: "TenantsPrograms");
        }
    }
}
