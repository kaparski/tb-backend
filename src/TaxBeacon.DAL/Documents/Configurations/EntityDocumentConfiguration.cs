namespace TaxBeacon.DAL.Documents.Configurations;

public class EntityDocumentConfiguration: IEntityTypeConfiguration<EntityDocument>
{
    public void Configure(EntityTypeBuilder<EntityDocument> builder)
    {
        builder
            .HasOne<Entity>(ed => ed.Entity)
            .WithMany(e => e.EntityDocuments)
            .HasForeignKey(ed => new { ed.TenantId, ed.EntityId });

        builder
            .HasOne<Document>(ed => ed.Document)
            .WithMany(d => d.EntityDocuments)
            .HasForeignKey(ed => new { ed.TenantId, ed.DocumentId })
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Tenant>(ed => ed.Tenant)
            .WithMany(t => t.EntityDocuments)
            .HasForeignKey(ed => ed.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasKey(ed => new { ed.TenantId, ed.EntityId, ed.DocumentId });
    }
}
