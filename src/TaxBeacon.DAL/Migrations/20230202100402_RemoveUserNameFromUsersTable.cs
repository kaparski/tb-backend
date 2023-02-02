using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserNameFromUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(202)",
                maxLength: 202,
                nullable: false,
                computedColumnSql: "TRIM(CONCAT([FirstName], ' ', [LastName]))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(202)",
                oldMaxLength: 202,
                oldComputedColumnSql: "CONCAT([FirstName], ' ', [LastName])",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(202)",
                maxLength: 202,
                nullable: false,
                computedColumnSql: "CONCAT([FirstName], ' ', [LastName])",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(202)",
                oldMaxLength: 202,
                oldComputedColumnSql: "TRIM(CONCAT([FirstName], ' ', [LastName]))",
                oldStored: true);
        }
    }
}
