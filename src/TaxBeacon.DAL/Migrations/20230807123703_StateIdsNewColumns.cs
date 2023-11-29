using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class StateIdsNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds");

            migrationBuilder.DropIndex(
                name: "IX_StateIds_EntityId",
                table: "StateIds");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "StateIds");

            migrationBuilder.AddColumn<string>(
                name: "LocalJurisdiction",
                table: "StateIds",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "StateIds",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StateIdCode",
                table: "StateIds",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StateIdType",
                table: "StateIds",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "StateIds",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_StateIds_EntityId_State",
                table: "StateIds",
                columns: new[] { "EntityId", "State" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StateIds_TenantId_EntityId_Id",
                table: "StateIds",
                columns: new[] { "TenantId", "EntityId", "Id" })
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_StateIds_TenantId_StateIdCode",
                table: "StateIds",
                columns: new[] { "TenantId", "StateIdCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StateIds_Tenants_TenantId",
                table: "StateIds",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StateIds_Tenants_TenantId",
                table: "StateIds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds");

            migrationBuilder.DropIndex(
                name: "IX_StateIds_EntityId_State",
                table: "StateIds");

            migrationBuilder.DropIndex(
                name: "IX_StateIds_TenantId_EntityId_Id",
                table: "StateIds");

            migrationBuilder.DropIndex(
                name: "IX_StateIds_TenantId_StateIdCode",
                table: "StateIds");

            migrationBuilder.DropColumn(
                name: "LocalJurisdiction",
                table: "StateIds");

            migrationBuilder.DropColumn(
                name: "State",
                table: "StateIds");

            migrationBuilder.DropColumn(
                name: "StateIdCode",
                table: "StateIds");

            migrationBuilder.DropColumn(
                name: "StateIdType",
                table: "StateIds");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StateIds");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "StateIds",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StateIds_EntityId",
                table: "StateIds",
                column: "EntityId");
        }
    }
}
