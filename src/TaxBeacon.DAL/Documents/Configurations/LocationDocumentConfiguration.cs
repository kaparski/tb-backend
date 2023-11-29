namespace TaxBeacon.DAL.Documents.Configurations;

public class LocationDocumentConfiguration: IEntityTypeConfiguration<LocationDocument>
{
    public void Configure(EntityTypeBuilder<LocationDocument> builder)
    {
        builder
            .HasOne<Location>(ld => ld.Location)
            .WithMany(l => l.LocationDocuments)
            .HasForeignKey(ld => new { ld.TenantId, ld.LocationId });

        builder
            .HasOne<Document>(ld => ld.Document)
            .WithMany(d => d.LocationDocuments)
            .HasForeignKey(ld => new { ld.TenantId, ld.DocumentId })
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Tenant>(ld => ld.Tenant)
            .WithMany(t => t.LocationDocuments)
            .HasForeignKey(ld => ld.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasKey(ld => new { ld.TenantId, ld.LocationId, ld.DocumentId });
    }
}
