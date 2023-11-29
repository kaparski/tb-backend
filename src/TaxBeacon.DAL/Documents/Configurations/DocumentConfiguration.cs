namespace TaxBeacon.DAL.Documents.Configurations;

public class DocumentConfiguration: IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder
            .Property(d => d.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(260)
            .IsRequired();

        builder
            .Property(d => d.Url)
            .HasColumnType("nvarchar")
            .HasMaxLength(1000)
            .IsRequired();

        builder
            .Property(d => d.ContentLength)
            .IsRequired();

        builder
            .Property(d => d.Id)
            .HasDefaultValueSql("NEWID()");

        builder
            .HasKey(d => new { d.TenantId, d.Id });

        builder
            .Property(d => d.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        builder
            .HasOne<Tenant>(d => d.Tenant)
            .WithMany(t => t.Documents)
            .HasForeignKey(d => d.TenantId);

        builder
            .HasOne<TenantUser>(d => d.TenantUser)
            .WithMany(tu => tu.Documents)
            .HasForeignKey(d => new { d.TenantId, d.UserId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
