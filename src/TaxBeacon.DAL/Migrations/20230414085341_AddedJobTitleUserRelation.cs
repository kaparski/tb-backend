using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedJobTitleUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobTitleId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_JobTitleId",
                table: "Users",
                column: "JobTitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_JobTitles_JobTitleId",
                table: "Users",
                column: "JobTitleId",
                principalTable: "JobTitles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_JobTitles_JobTitleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_JobTitleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitleId",
                table: "Users");
        }
    }
}
