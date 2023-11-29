using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAccountContactTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivationDateTimeUtc",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ReactivationDateTimeUtc",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Contacts");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivationDateTimeUtc",
                table: "AccountContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivationDateTimeUtc",
                table: "AccountContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AccountContacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "AccountContacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivationDateTimeUtc",
                table: "AccountContacts");

            migrationBuilder.DropColumn(
                name: "ReactivationDateTimeUtc",
                table: "AccountContacts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AccountContacts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "AccountContacts");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivationDateTimeUtc",
                table: "Contacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivationDateTimeUtc",
                table: "Contacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Contacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Contacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
