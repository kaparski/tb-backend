using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedJurisditionNameColumnToPrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JurisdictionName",
                table: "Programs",
                type: "nvarchar(602)",
                maxLength: 602,
                nullable: true,
                computedColumnSql: "TRIM(CASE WHEN [Jurisdiction] = 1 THEN 'Federal' WHEN [Jurisdiction] = 2 THEN [State] WHEN [Jurisdiction] = 3 THEN CONCAT([State], ',', [County], ',', [City]) ELSE NULL END)",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JurisdictionName",
                table: "Programs");
        }
    }
}
