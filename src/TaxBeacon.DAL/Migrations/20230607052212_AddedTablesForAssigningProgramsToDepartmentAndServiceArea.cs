using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedTablesForAssigningProgramsToDepartmentAndServiceArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentTenantPrograms",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    DeletedDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentTenantPrograms", x => new { x.TenantId, x.DepartmentId, x.ProgramId });
                    table.ForeignKey(
                        name: "FK_DepartmentTenantPrograms_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentTenantPrograms_TenantsPrograms_TenantId_ProgramId",
                        columns: x => new { x.TenantId, x.ProgramId },
                        principalTable: "TenantsPrograms",
                        principalColumns: new[] { "TenantId", "ProgramId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAreaTenantPrograms",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    DeletedDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAreaTenantPrograms", x => new { x.TenantId, x.ServiceAreaId, x.ProgramId });
                    table.ForeignKey(
                        name: "FK_ServiceAreaTenantPrograms_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceAreaTenantPrograms_TenantsPrograms_TenantId_ProgramId",
                        columns: x => new { x.TenantId, x.ProgramId },
                        principalTable: "TenantsPrograms",
                        principalColumns: new[] { "TenantId", "ProgramId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentTenantPrograms_DepartmentId",
                table: "DepartmentTenantPrograms",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentTenantPrograms_TenantId_ProgramId",
                table: "DepartmentTenantPrograms",
                columns: new[] { "TenantId", "ProgramId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreaTenantPrograms_ServiceAreaId",
                table: "ServiceAreaTenantPrograms",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreaTenantPrograms_TenantId_ProgramId",
                table: "ServiceAreaTenantPrograms",
                columns: new[] { "TenantId", "ProgramId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentTenantPrograms");

            migrationBuilder.DropTable(
                name: "ServiceAreaTenantPrograms");
        }
    }
}
