using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedJurisdictionNameColumnForPrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JurisdictionName",
                table: "Programs",
                type: "nvarchar(604)",
                maxLength: 604,
                nullable: true,
                computedColumnSql: "TRIM(CASE WHEN [Jurisdiction] = 1 THEN 'Federal' WHEN [Jurisdiction] = 2 THEN [State] WHEN [Jurisdiction] = 3 THEN CONCAT_WS(', ',[State], [County], [City]) ELSE NULL END)",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(604)",
                oldMaxLength: 604,
                oldNullable: true,
                oldComputedColumnSql: "TRIM(CASE WHEN [Jurisdiction] = 1 THEN 'Federal' WHEN [Jurisdiction] = 2 THEN [State] WHEN [Jurisdiction] = 3 THEN CONCAT([State], ', ', [County], ', ', [City]) ELSE NULL END)",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JurisdictionName",
                table: "Programs",
                type: "nvarchar(604)",
                maxLength: 604,
                nullable: true,
                computedColumnSql: "TRIM(CASE WHEN [Jurisdiction] = 1 THEN 'Federal' WHEN [Jurisdiction] = 2 THEN [State] WHEN [Jurisdiction] = 3 THEN CONCAT([State], ', ', [County], ', ', [City]) ELSE NULL END)",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(604)",
                oldMaxLength: 604,
                oldNullable: true,
                oldComputedColumnSql: "TRIM(CASE WHEN [Jurisdiction] = 1 THEN 'Federal' WHEN [Jurisdiction] = 2 THEN [State] WHEN [Jurisdiction] = 3 THEN CONCAT_WS(', ',[State], [County], [City]) ELSE NULL END)",
                oldStored: true);
        }
    }
}
