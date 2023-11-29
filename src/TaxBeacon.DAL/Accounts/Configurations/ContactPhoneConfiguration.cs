using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Accounts.Extensions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class ContactPhoneConfiguration: IEntityTypeConfiguration<ContactPhone>
{
    public void Configure(EntityTypeBuilder<ContactPhone> contactPhone)
    {
        contactPhone.ConfigurePhone();

        contactPhone
            .HasOne<Contact>(p => p.Contact)
            .WithMany(l => l.Phones)
            .HasForeignKey(p => new { p.TenantId, p.ContactId });

        contactPhone
            .HasOne<Tenant>(p => p.Tenant)
            .WithMany(t => t.ContactPhones)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
