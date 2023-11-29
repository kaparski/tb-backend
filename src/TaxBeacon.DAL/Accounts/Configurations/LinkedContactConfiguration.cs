using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class LinkedContactConfiguration: IEntityTypeConfiguration<LinkedContact>
{
    public void Configure(EntityTypeBuilder<LinkedContact> linkedContact)
    {
        linkedContact
            .HasOne<Contact>(rc => rc.SourceContact)
            .WithMany(c => c.LinkedContacts)
            .HasForeignKey(rc => new { rc.TenantId, rc.SourceContactId });

        linkedContact
            .HasOne<Contact>(rc => rc.RelatedContact)
            .WithMany()
            .HasForeignKey(rc => new { rc.TenantId, rc.RelatedContactId })
            .OnDelete(DeleteBehavior.NoAction);

        linkedContact
            .HasOne<Tenant>(rc => rc.Tenant)
            .WithMany(t => t.LinkedContacts)
            .HasForeignKey(rc => rc.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        linkedContact
            .Property(lc => lc.Comment)
            .HasColumnType("nvarchar")
            .HasMaxLength(150)
            .HasConversion<string>();

        linkedContact
            .HasKey(rc => new { rc.SourceContactId, rc.RelatedContactId });
    }
}
