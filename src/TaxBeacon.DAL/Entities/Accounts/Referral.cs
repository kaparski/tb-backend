﻿using TaxBeacon.Common.Accounts;

namespace TaxBeacon.DAL.Entities.Accounts;
public class Referral: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public ReferralState State { get; set; }

    public bool IsEnabled { get; set; }

    public Guid? ManagerId { get; set; }

    public User? Manager { get; set; }

    public string? NaicsCode { get; set; }
}