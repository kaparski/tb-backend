﻿using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Entity: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Dba { get; set; }

    public string? EntityId { get; set; }

    public string? City { get; set; }

    public string StreetAddress1 { get; set; } = null!;

    public string? StreetAddress2 { get; set; }

    public string? Address { get; set; }

    public int Fein { get; set; }

    public int Zip { get; set; }

    public string Country { get; set; } = null!;

    public State? State { get; set; }

    public string Type { get; set; } = null!;

    public TaxYearEndType? TaxYearEndType { get; set; }

    public string? Fax { get; set; }

    public string? Phone { get; set; }

    public string? Extension { get; set; }

    public Status Status { get; set; }

    public ICollection<StateId> StateIds { get; } = new HashSet<StateId>();

    public ICollection<EntityActivityLog> EntityActivityLogs { get; set; } = new HashSet<EntityActivityLog>();
}
