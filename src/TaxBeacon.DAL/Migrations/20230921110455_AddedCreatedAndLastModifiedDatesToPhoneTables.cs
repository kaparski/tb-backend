using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedCreatedAndLastModifiedDatesToPhoneTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTimeUtc",
                table: "LocationPhones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDateTimeUtc",
                table: "LocationPhones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTimeUtc",
                table: "EntityPhones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDateTimeUtc",
                table: "EntityPhones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTimeUtc",
                table: "AccountPhones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDateTimeUtc",
                table: "AccountPhones",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDateTimeUtc",
                table: "LocationPhones");

            migrationBuilder.DropColumn(
                name: "LastModifiedDateTimeUtc",
                table: "LocationPhones");

            migrationBuilder.DropColumn(
                name: "CreatedDateTimeUtc",
                table: "EntityPhones");

            migrationBuilder.DropColumn(
                name: "LastModifiedDateTimeUtc",
                table: "EntityPhones");

            migrationBuilder.DropColumn(
                name: "CreatedDateTimeUtc",
                table: "AccountPhones");

            migrationBuilder.DropColumn(
                name: "LastModifiedDateTimeUtc",
                table: "AccountPhones");
        }
    }
}
