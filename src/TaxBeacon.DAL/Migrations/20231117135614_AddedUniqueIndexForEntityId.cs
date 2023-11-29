using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedUniqueIndexForEntityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Entities
SET EntityId = 'E' + STR(ABS(CAST(CRYPT_GEN_RANDOM(4) AS INT))%(9999999-1000000)+1000000, 7, 0)
WHERE EntityId IS NULL or EntityId = ''");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_TenantId_EntityId",
                table: "Entities",
                columns: new[] { "TenantId", "EntityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entities_TenantId_EntityId",
                table: "Entities");
        }
    }
}
