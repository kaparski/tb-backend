using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;
public class ReferralViewConfiguration: IEntityTypeConfiguration<ReferralView>
{
    public void Configure(EntityTypeBuilder<ReferralView> referralView)
    {
        referralView.HasKey(x => new
        {
            x.TenantId,
            x.AccountId
        });

        referralView
            .Property(a => a.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2) // maybe it should be 4 for 'none'?
            .HasConversion<string>();

        referralView
            .Property(a => a.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        referralView
            .Property(a => a.ReferralState)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        referralView
            .Property(a => a.OrganizationType)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(50);

        referralView
            .Property(a => a.Type)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(50);

        referralView.ToTable("ReferralsView", t => t.ExcludeFromMigrations());
    }
}
