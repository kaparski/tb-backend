using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class AccountContactConfiguration: IEntityTypeConfiguration<AccountContact>
{
    public void Configure(EntityTypeBuilder<AccountContact> accountContact)
    {
        accountContact
            .HasOne<Account>(ac => ac.Account)
            .WithMany(a => a.Contacts)
            .HasForeignKey(ac => new { ac.TenantId, ac.AccountId });

        accountContact
            .HasOne<Contact>(ac => ac.Contact)
            .WithMany(c => c.Accounts)
            .HasForeignKey(ac => new { ac.TenantId, ac.ContactId });

        accountContact
            .HasOne<Tenant>(acl => acl.Tenant)
            .WithMany(t => t.AccountContacts)
            .HasForeignKey(acl => acl.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        accountContact
            .Property(ac => ac.Type)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        accountContact
            .Property(ac => ac.Status)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .HasConversion<string>();

        accountContact.HasKey(ac => new { ac.TenantId, ac.AccountId, ac.ContactId });
    }
}
