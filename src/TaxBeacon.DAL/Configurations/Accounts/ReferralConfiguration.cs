﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Configurations.Accounts;
public class ReferralConfiguration: IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> referral)
    {
        referral
            .HasKey(r => new { r.TenantId, r.AccountId });

        referral
            .HasOne(r => r.Tenant)
            .WithMany(t => t.Referrals)
            .HasForeignKey(r => r.TenantId);

        referral
            .HasOne(r => r.Account)
            .WithOne(a => a.Referral)
            .HasForeignKey<Referral>(r => r.AccountId);

        referral
            .HasOne(r => r.Manager)
            .WithMany(m => m.Referrals)
            .HasForeignKey(r => r.ManagerId);
    }
}