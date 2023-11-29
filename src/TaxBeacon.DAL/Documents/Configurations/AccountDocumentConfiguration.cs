namespace TaxBeacon.DAL.Documents.Configurations;

public class AccountDocumentConfiguration: IEntityTypeConfiguration<AccountDocument>
{
    public void Configure(EntityTypeBuilder<AccountDocument> builder)
    {
        builder
            .HasOne<Account>(ad => ad.Account)
            .WithMany(a => a.AccountDocuments)
            .HasForeignKey(ad => new { ad.TenantId, ad.AccountId });

        builder
            .HasOne<Document>(ad => ad.Document)
            .WithMany(d => d.AccountDocuments)
            .HasForeignKey(ad => new { ad.TenantId, ad.DocumentId })
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Tenant>(ad => ad.Tenant)
            .WithMany(t => t.AccountDocuments)
            .HasForeignKey(ad => ad.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasKey(ad => new { ad.TenantId, ad.AccountId, ad.DocumentId });
    }
}
