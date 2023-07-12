using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDaysOpenQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DaysOpen",
                table: "Clients",
                type: "int",
                nullable: false,
                computedColumnSql: "DATEDIFF(second, COALESCE(ActivationDateTimeUtc, CreatedDateTimeUtc), COALESCE(DeactivationDateTimeUtc, GETUTCDATE())) / 86400",
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "DATEDIFF(second, COALESCE(ActivationDateTimeUtc, CreatedDateTimeUtc), GETUTCDATE()) / 86400");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DaysOpen",
                table: "Clients",
                type: "int",
                nullable: false,
                computedColumnSql: "DATEDIFF(second, COALESCE(ActivationDateTimeUtc, CreatedDateTimeUtc), GETUTCDATE()) / 86400",
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "DATEDIFF(second, COALESCE(ActivationDateTimeUtc, CreatedDateTimeUtc), COALESCE(DeactivationDateTimeUtc, GETUTCDATE())) / 86400");
        }
    }
}
