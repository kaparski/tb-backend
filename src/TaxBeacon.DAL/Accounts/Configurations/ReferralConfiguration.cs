namespace TaxBeacon.DAL.Accounts.Configurations;

public class ReferralConfiguration: IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> referral)
    {
        referral
            .HasKey(r => new { r.TenantId, r.AccountId });

        referral
            .HasOne(r => r.Tenant)
            .WithMany(t => t.Referrals)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        referral
            .HasOne(r => r.Account)
            .WithOne(a => a.Referral)
            .HasForeignKey<Referral>(r => new { r.TenantId, r.AccountId });

        referral
            .HasOne(c => c.PrimaryContact)
            .WithMany(m => m.Referrals)
            .HasForeignKey(c => new { c.TenantId, c.PrimaryContactId });

        referral
            .Property(r => r.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        referral
            .Property(c => c.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(50)
            .IsRequired();

        referral
            .Property(c => c.OrganizationType)
            .HasColumnType("nvarchar")
            .HasMaxLength(50)
            .IsRequired();

        referral
            .Property(c => c.Type)
            .HasColumnType("nvarchar")
            .HasMaxLength(50)
            .IsRequired();

        referral
            .Property(u => u.DaysOpen)
            .HasComputedColumnSql(
                "DATEDIFF(second, COALESCE(ActivationDateTimeUtc, CreatedDateTimeUtc), COALESCE(DeactivationDateTimeUtc, GETUTCDATE())) / 86400");

    }
}
