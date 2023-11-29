namespace TaxBeacon.DAL.Accounts.Configurations;
public class ReferralManagerConfiguration: IEntityTypeConfiguration<ReferralManager>
{
    public void Configure(EntityTypeBuilder<ReferralManager> referralManager)
    {
        referralManager
            .HasOne<Referral>(cm => cm.Referral)
            .WithMany(c => c.ReferralManagers)
            .HasForeignKey(cm => new { cm.TenantId, cm.AccountId })
            .OnDelete(DeleteBehavior.NoAction);

        referralManager
            .HasOne<TenantUser>(cm => cm.TenantUser)
            .WithMany(u => u.ReferralManagers)
            .HasForeignKey(cm => new { cm.TenantId, cm.UserId })
            .OnDelete(DeleteBehavior.NoAction);

        referralManager
            .HasKey(cm => new { cm.TenantId, cm.AccountId, cm.UserId });
    }
}

