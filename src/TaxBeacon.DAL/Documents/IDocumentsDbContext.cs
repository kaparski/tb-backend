namespace TaxBeacon.DAL.Documents;

public interface IDocumentsDbContext
{
    DbSet<Document> Documents { get; }

    DbSet<AccountDocument> AccountDocuments { get; }

    DbSet<EntityDocument> EntityDocuments { get; }

    DbSet<LocationDocument> LocationDocuments { get; }
}
