using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedTenantIdInAccountModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountActivityLogs_Accounts_AccountId",
                table: "AccountActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountContacts_Accounts_AccountId",
                table: "AccountContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountContacts_Contacts_ContactId",
                table: "AccountContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountPhones_Accounts_AccountId",
                table: "AccountPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Accounts_AccountId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Contacts_PrimaryContactId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactActivityLogs_Contacts_ContactId",
                table: "ContactActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhones_Contacts_ContactId",
                table: "ContactPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Entities_Accounts_AccountId",
                table: "Entities");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityActivityLogs_Entities_EntityId",
                table: "EntityActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityLocations_Entities_EntityId",
                table: "EntityLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityLocations_Locations_LocationId",
                table: "EntityLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityPhones_Entities_EntityId",
                table: "EntityPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkedContacts_Contacts_RelatedContactId",
                table: "LinkedContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkedContacts_Contacts_SourceContactId",
                table: "LinkedContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationActivityLogs_Locations_LocationId",
                table: "LocationActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationPhones_Locations_LocationId",
                table: "LocationPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Accounts_AccountId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Accounts_AccountId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Salespersons_Accounts_AccountId",
                table: "Salespersons");

            migrationBuilder.DropForeignKey(
                name: "FK_StateIds_Entities_EntityId",
                table: "StateIds");

            migrationBuilder.DropForeignKey(
                name: "FK_StateIds_Tenants_TenantId",
                table: "StateIds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds");

            migrationBuilder.DropIndex(
                name: "IX_Salespersons_AccountId",
                table: "Salespersons");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_AccountId",
                table: "Referrals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_AccountId",
                table: "Locations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationPhones",
                table: "LocationPhones");

            migrationBuilder.DropIndex(
                name: "IX_LocationPhones_LocationId",
                table: "LocationPhones");

            migrationBuilder.DropIndex(
                name: "IX_LocationActivityLogs_LocationId",
                table: "LocationActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_LinkedContacts_RelatedContactId",
                table: "LinkedContacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntityPhones",
                table: "EntityPhones");

            migrationBuilder.DropIndex(
                name: "IX_EntityPhones_EntityId",
                table: "EntityPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntityLocations",
                table: "EntityLocations");

            migrationBuilder.DropIndex(
                name: "IX_EntityLocations_LocationId",
                table: "EntityLocations");

            migrationBuilder.DropIndex(
                name: "IX_EntityActivityLogs_EntityId",
                table: "EntityActivityLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entities",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_Entities_AccountId",
                table: "Entities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactPhones",
                table: "ContactPhones");

            migrationBuilder.DropIndex(
                name: "IX_ContactPhones_ContactId",
                table: "ContactPhones");

            migrationBuilder.DropIndex(
                name: "IX_ContactActivityLogs_ContactId",
                table: "ContactActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_Clients_AccountId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_PrimaryContactId",
                table: "Clients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_TenantId_Id",
                table: "Accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountPhones",
                table: "AccountPhones");

            migrationBuilder.DropIndex(
                name: "IX_AccountPhones_AccountId",
                table: "AccountPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountContacts",
                table: "AccountContacts");

            migrationBuilder.DropIndex(
                name: "IX_AccountContacts_ContactId",
                table: "AccountContacts");

            migrationBuilder.DropIndex(
                name: "IX_AccountActivityLogs_AccountId",
                table: "AccountActivityLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "LocationPhones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "LinkedContacts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EntityPhones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EntityLocations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ContactPhones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AccountPhones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AccountContacts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds",
                columns: new[] { "TenantId", "Id" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                columns: new[] { "TenantId", "Id" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationPhones",
                table: "LocationPhones",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntityPhones",
                table: "EntityPhones",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntityLocations",
                table: "EntityLocations",
                columns: new[] { "TenantId", "EntityId", "LocationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entities",
                table: "Entities",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactPhones",
                table: "ContactPhones",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountPhones",
                table: "AccountPhones",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountContacts",
                table: "AccountContacts",
                columns: new[] { "TenantId", "AccountId", "ContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationPhones_TenantId_LocationId",
                table: "LocationPhones",
                columns: new[] { "TenantId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_LinkedContacts_TenantId_RelatedContactId",
                table: "LinkedContacts",
                columns: new[] { "TenantId", "RelatedContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_LinkedContacts_TenantId_SourceContactId",
                table: "LinkedContacts",
                columns: new[] { "TenantId", "SourceContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityPhones_TenantId_EntityId",
                table: "EntityPhones",
                columns: new[] { "TenantId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityLocations_TenantId_LocationId",
                table: "EntityLocations",
                columns: new[] { "TenantId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhones_TenantId_ContactId",
                table: "ContactPhones",
                columns: new[] { "TenantId", "ContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_PrimaryContactId",
                table: "Clients",
                columns: new[] { "TenantId", "PrimaryContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountPhones_TenantId_AccountId",
                table: "AccountPhones",
                columns: new[] { "TenantId", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountContacts_TenantId_ContactId",
                table: "AccountContacts",
                columns: new[] { "TenantId", "ContactId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AccountActivityLogs_Accounts_TenantId_AccountId",
                table: "AccountActivityLogs",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountContacts_Accounts_TenantId_AccountId",
                table: "AccountContacts",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountContacts_Contacts_TenantId_ContactId",
                table: "AccountContacts",
                columns: new[] { "TenantId", "ContactId" },
                principalTable: "Contacts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountContacts_Tenants_TenantId",
                table: "AccountContacts",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPhones_Accounts_TenantId_AccountId",
                table: "AccountPhones",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPhones_Tenants_TenantId",
                table: "AccountPhones",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Accounts_TenantId_AccountId",
                table: "Clients",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Contacts_TenantId_PrimaryContactId",
                table: "Clients",
                columns: new[] { "TenantId", "PrimaryContactId" },
                principalTable: "Contacts",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ContactActivityLogs_Contacts_TenantId_ContactId",
                table: "ContactActivityLogs",
                columns: new[] { "TenantId", "ContactId" },
                principalTable: "Contacts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhones_Contacts_TenantId_ContactId",
                table: "ContactPhones",
                columns: new[] { "TenantId", "ContactId" },
                principalTable: "Contacts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhones_Tenants_TenantId",
                table: "ContactPhones",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_Accounts_TenantId_AccountId",
                table: "Entities",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityActivityLogs_Entities_TenantId_EntityId",
                table: "EntityActivityLogs",
                columns: new[] { "TenantId", "EntityId" },
                principalTable: "Entities",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityLocations_Entities_TenantId_EntityId",
                table: "EntityLocations",
                columns: new[] { "TenantId", "EntityId" },
                principalTable: "Entities",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityLocations_Locations_TenantId_LocationId",
                table: "EntityLocations",
                columns: new[] { "TenantId", "LocationId" },
                principalTable: "Locations",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_EntityLocations_Tenants_TenantId",
                table: "EntityLocations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityPhones_Entities_TenantId_EntityId",
                table: "EntityPhones",
                columns: new[] { "TenantId", "EntityId" },
                principalTable: "Entities",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_EntityPhones_Tenants_TenantId",
                table: "EntityPhones",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedContacts_Contacts_TenantId_RelatedContactId",
                table: "LinkedContacts",
                columns: new[] { "TenantId", "RelatedContactId" },
                principalTable: "Contacts",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedContacts_Contacts_TenantId_SourceContactId",
                table: "LinkedContacts",
                columns: new[] { "TenantId", "SourceContactId" },
                principalTable: "Contacts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedContacts_Tenants_TenantId",
                table: "LinkedContacts",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationActivityLogs_Locations_TenantId_LocationId",
                table: "LocationActivityLogs",
                columns: new[] { "TenantId", "LocationId" },
                principalTable: "Locations",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPhones_Locations_TenantId_LocationId",
                table: "LocationPhones",
                columns: new[] { "TenantId", "LocationId" },
                principalTable: "Locations",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPhones_Tenants_TenantId",
                table: "LocationPhones",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Accounts_TenantId_AccountId",
                table: "Locations",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Accounts_TenantId_AccountId",
                table: "Referrals",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Salespersons_Accounts_TenantId_AccountId",
                table: "Salespersons",
                columns: new[] { "TenantId", "AccountId" },
                principalTable: "Accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StateIds_Entities_TenantId_EntityId",
                table: "StateIds",
                columns: new[] { "TenantId", "EntityId" },
                principalTable: "Entities",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StateIds_Tenants_TenantId",
                table: "StateIds",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountActivityLogs_Accounts_TenantId_AccountId",
                table: "AccountActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountContacts_Accounts_TenantId_AccountId",
                table: "AccountContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountContacts_Contacts_TenantId_ContactId",
                table: "AccountContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountContacts_Tenants_TenantId",
                table: "AccountContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountPhones_Accounts_TenantId_AccountId",
                table: "AccountPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountPhones_Tenants_TenantId",
                table: "AccountPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Accounts_TenantId_AccountId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Contacts_TenantId_PrimaryContactId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactActivityLogs_Contacts_TenantId_ContactId",
                table: "ContactActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhones_Contacts_TenantId_ContactId",
                table: "ContactPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhones_Tenants_TenantId",
                table: "ContactPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Entities_Accounts_TenantId_AccountId",
                table: "Entities");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityActivityLogs_Entities_TenantId_EntityId",
                table: "EntityActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityLocations_Entities_TenantId_EntityId",
                table: "EntityLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityLocations_Locations_TenantId_LocationId",
                table: "EntityLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityLocations_Tenants_TenantId",
                table: "EntityLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityPhones_Entities_TenantId_EntityId",
                table: "EntityPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityPhones_Tenants_TenantId",
                table: "EntityPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkedContacts_Contacts_TenantId_RelatedContactId",
                table: "LinkedContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkedContacts_Contacts_TenantId_SourceContactId",
                table: "LinkedContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkedContacts_Tenants_TenantId",
                table: "LinkedContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationActivityLogs_Locations_TenantId_LocationId",
                table: "LocationActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationPhones_Locations_TenantId_LocationId",
                table: "LocationPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationPhones_Tenants_TenantId",
                table: "LocationPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Accounts_TenantId_AccountId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Accounts_TenantId_AccountId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Salespersons_Accounts_TenantId_AccountId",
                table: "Salespersons");

            migrationBuilder.DropForeignKey(
                name: "FK_StateIds_Entities_TenantId_EntityId",
                table: "StateIds");

            migrationBuilder.DropForeignKey(
                name: "FK_StateIds_Tenants_TenantId",
                table: "StateIds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationPhones",
                table: "LocationPhones");

            migrationBuilder.DropIndex(
                name: "IX_LocationPhones_TenantId_LocationId",
                table: "LocationPhones");

            migrationBuilder.DropIndex(
                name: "IX_LinkedContacts_TenantId_RelatedContactId",
                table: "LinkedContacts");

            migrationBuilder.DropIndex(
                name: "IX_LinkedContacts_TenantId_SourceContactId",
                table: "LinkedContacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntityPhones",
                table: "EntityPhones");

            migrationBuilder.DropIndex(
                name: "IX_EntityPhones_TenantId_EntityId",
                table: "EntityPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntityLocations",
                table: "EntityLocations");

            migrationBuilder.DropIndex(
                name: "IX_EntityLocations_TenantId_LocationId",
                table: "EntityLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entities",
                table: "Entities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactPhones",
                table: "ContactPhones");

            migrationBuilder.DropIndex(
                name: "IX_ContactPhones_TenantId_ContactId",
                table: "ContactPhones");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_PrimaryContactId",
                table: "Clients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountPhones",
                table: "AccountPhones");

            migrationBuilder.DropIndex(
                name: "IX_AccountPhones_TenantId_AccountId",
                table: "AccountPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountContacts",
                table: "AccountContacts");

            migrationBuilder.DropIndex(
                name: "IX_AccountContacts_TenantId_ContactId",
                table: "AccountContacts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LocationPhones");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LinkedContacts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EntityPhones");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EntityLocations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ContactPhones");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AccountPhones");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AccountContacts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateIds",
                table: "StateIds",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationPhones",
                table: "LocationPhones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntityPhones",
                table: "EntityPhones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntityLocations",
                table: "EntityLocations",
                columns: new[] { "EntityId", "LocationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entities",
                table: "Entities",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactPhones",
                table: "ContactPhones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountPhones",
                table: "AccountPhones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountContacts",
                table: "AccountContacts",
                columns: new[] { "AccountId", "ContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_Salespersons_AccountId",
                table: "Salespersons",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_AccountId",
                table: "Referrals",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_AccountId",
                table: "Locations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationPhones_LocationId",
                table: "LocationPhones",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationActivityLogs_LocationId",
                table: "LocationActivityLogs",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedContacts_RelatedContactId",
                table: "LinkedContacts",
                column: "RelatedContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityPhones_EntityId",
                table: "EntityPhones",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLocations_LocationId",
                table: "EntityLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityActivityLogs_EntityId",
                table: "EntityActivityLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_AccountId",
                table: "Entities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhones_ContactId",
                table: "ContactPhones",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactActivityLogs_ContactId",
                table: "ContactActivityLogs",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AccountId",
                table: "Clients",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_PrimaryContactId",
                table: "Clients",
                column: "PrimaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId_Id",
                table: "Accounts",
                columns: new[] { "TenantId", "Id" })
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountPhones_AccountId",
                table: "AccountPhones",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountContacts_ContactId",
                table: "AccountContacts",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountActivityLogs_AccountId",
                table: "AccountActivityLogs",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountActivityLogs_Accounts_AccountId",
                table: "AccountActivityLogs",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountContacts_Accounts_AccountId",
                table: "AccountContacts",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountContacts_Contacts_ContactId",
                table: "AccountContacts",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPhones_Accounts_AccountId",
                table: "AccountPhones",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Accounts_AccountId",
                table: "Clients",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Contacts_PrimaryContactId",
                table: "Clients",
                column: "PrimaryContactId",
                principalTable: "Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactActivityLogs_Contacts_ContactId",
                table: "ContactActivityLogs",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhones_Contacts_ContactId",
                table: "ContactPhones",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_Accounts_AccountId",
                table: "Entities",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityActivityLogs_Entities_EntityId",
                table: "EntityActivityLogs",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityLocations_Entities_EntityId",
                table: "EntityLocations",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityLocations_Locations_LocationId",
                table: "EntityLocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityPhones_Entities_EntityId",
                table: "EntityPhones",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedContacts_Contacts_RelatedContactId",
                table: "LinkedContacts",
                column: "RelatedContactId",
                principalTable: "Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedContacts_Contacts_SourceContactId",
                table: "LinkedContacts",
                column: "SourceContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationActivityLogs_Locations_LocationId",
                table: "LocationActivityLogs",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPhones_Locations_LocationId",
                table: "LocationPhones",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Accounts_AccountId",
                table: "Locations",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Accounts_AccountId",
                table: "Referrals",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Salespersons_Accounts_AccountId",
                table: "Salespersons",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StateIds_Entities_EntityId",
                table: "StateIds",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StateIds_Tenants_TenantId",
                table: "StateIds",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
