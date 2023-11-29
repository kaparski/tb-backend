using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangedNaicsCodeTypeFromStringToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_NaicsCodes_NaicsCodes_ParentCode", "NaicsCodes");
            migrationBuilder.DropIndex("IX_NaicsCodes_ParentCode", "NaicsCodes");
            migrationBuilder.DropPrimaryKey("PK_NaicsCodes", "NaicsCodes");

            migrationBuilder.AlterColumn<int>(
                name: "ParentCode",
                table: "NaicsCodes",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Code",
                table: "NaicsCodes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NaicsCodes",
                table: "NaicsCodes",
                column: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_NaicsCodes_NaicsCodes_ParentCode",
                table: "NaicsCodes",
                column: "ParentCode",
                principalTable: "NaicsCodes",
                principalColumn: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_NaicsCodes_ParentCode",
                table: "NaicsCodes",
                column: "ParentCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_NaicsCodes_NaicsCodes_ParentCode", "NaicsCodes");
            migrationBuilder.DropIndex("IX_NaicsCodes_ParentCode", "NaicsCodes");
            migrationBuilder.DropPrimaryKey("PK_NaicsCodes", "NaicsCodes");

            migrationBuilder.AlterColumn<string>(
                name: "ParentCode",
                table: "NaicsCodes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "NaicsCodes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NaicsCodes",
                table: "NaicsCodes",
                column: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_NaicsCodes_NaicsCodes_ParentCode",
                table: "NaicsCodes",
                column: "ParentCode",
                principalTable: "NaicsCodes",
                principalColumn: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_NaicsCodes_ParentCode",
                table: "NaicsCodes",
                column: "ParentCode");
        }
    }
}
