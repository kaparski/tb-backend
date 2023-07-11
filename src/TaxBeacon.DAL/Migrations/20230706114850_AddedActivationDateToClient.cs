using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedActivationDateToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationDateTimeUtc",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysOpen",
                table: "Clients",
                type: "int",
                nullable: false,
                computedColumnSql: "DATEDIFF(second, COALESCE(ActivationDateTimeUtc, CreatedDateTimeUtc), GETUTCDATE()) / 86400");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysOpen",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ActivationDateTimeUtc",
                table: "Clients");
        }
    }
}
