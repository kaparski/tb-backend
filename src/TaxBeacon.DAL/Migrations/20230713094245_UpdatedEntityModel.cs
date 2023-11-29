using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEntityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entities_TenantId_Fein",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "Dba",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "StreetAddress1",
                table: "Entities");

            migrationBuilder.RenameColumn(
                name: "StreetAddress2",
                table: "Entities",
                newName: "County");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Entities",
                newName: "JurisdictionId");

            migrationBuilder.RenameColumn(
                name: "Fax",
                table: "Entities",
                newName: "Ein");

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                table: "Phones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Zip",
                table: "Entities",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Fein",
                table: "Entities",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Address1",
                table: "Entities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "Entities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfIncorporation",
                table: "Entities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoingBusinessAs",
                table: "Entities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Phones_EntityId",
                table: "Phones",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_TenantId_Ein",
                table: "Entities",
                columns: new[] { "TenantId", "Ein" },
                unique: true,
                filter: "[Ein] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_TenantId_Fein",
                table: "Entities",
                columns: new[] { "TenantId", "Fein" },
                unique: true,
                filter: "[Fein] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Phones_Entities_EntityId",
                table: "Phones",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phones_Entities_EntityId",
                table: "Phones");

            migrationBuilder.DropIndex(
                name: "IX_Phones_EntityId",
                table: "Phones");

            migrationBuilder.DropIndex(
                name: "IX_Entities_TenantId_Ein",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_Entities_TenantId_Fein",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Phones");

            migrationBuilder.DropColumn(
                name: "Address1",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "DateOfIncorporation",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "DoingBusinessAs",
                table: "Entities");

            migrationBuilder.RenameColumn(
                name: "JurisdictionId",
                table: "Entities",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "Ein",
                table: "Entities",
                newName: "Fax");

            migrationBuilder.RenameColumn(
                name: "County",
                table: "Entities",
                newName: "StreetAddress2");

            migrationBuilder.AlterColumn<string>(
                name: "Zip",
                table: "Entities",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Fein",
                table: "Entities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dba",
                table: "Entities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "Entities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Entities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress1",
                table: "Entities",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_TenantId_Fein",
                table: "Entities",
                columns: new[] { "TenantId", "Fein" },
                unique: true);
        }
    }
}
