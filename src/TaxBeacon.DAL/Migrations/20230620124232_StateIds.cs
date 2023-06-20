using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class StateIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityActivityLog_Entities_EntityId",
                table: "EntityActivityLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityActivityLog_Tenants_TenantId",
                table: "EntityActivityLog");

            migrationBuilder.DropForeignKey(
                name: "FK_StateId_Entities_EntityId",
                table: "StateId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StateId",
                table: "StateId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntityActivityLog",
                table: "EntityActivityLog");

            migrationBuilder.RenameTable(
                name: "StateId",
                newName: "StateIds");

            migrationBuilder.RenameTable(
                name: "EntityActivityLog",
                newName: "EntityActivityLogs");

            migrationBuilder.RenameIndex(
                name: "IX_StateId_EntityId",
                table: "StateIds",
                newName: "IX_StateIds_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_EntityActivityLog_EntityId",
                table: "EntityActivityLogs",
                newName: "IX_EntityActivityLogs_EntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntityActivityLogs",
                table: "EntityActivityLogs",
                columns: new[] { "TenantId", "EntityId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_EntityActivityLogs_Entities_EntityId",
                table: "EntityActivityLogs",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityActivityLogs_Tenants_TenantId",
                table: "EntityActivityLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StateIds_Entities_EntityId",
                table: "StateIds",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityActivityLogs_Entities_EntityId",
                table: "EntityActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityActivityLogs_Tenants_TenantId",
                table: "EntityActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_StateIds_Entities_EntityId",
                table: "StateIds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntityActivityLogs",
                table: "EntityActivityLogs");

            migrationBuilder.RenameTable(
                name: "StateIds",
                newName: "StateId");

            migrationBuilder.RenameTable(
                name: "EntityActivityLogs",
                newName: "EntityActivityLog");

            migrationBuilder.RenameIndex(
                name: "IX_StateIds_EntityId",
                table: "StateId",
                newName: "IX_StateId_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_EntityActivityLogs_EntityId",
                table: "EntityActivityLog",
                newName: "IX_EntityActivityLog_EntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateId",
                table: "StateId",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntityActivityLog",
                table: "EntityActivityLog",
                columns: new[] { "TenantId", "EntityId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_EntityActivityLog_Entities_EntityId",
                table: "EntityActivityLog",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityActivityLog_Tenants_TenantId",
                table: "EntityActivityLog",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StateId_Entities_EntityId",
                table: "StateId",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
