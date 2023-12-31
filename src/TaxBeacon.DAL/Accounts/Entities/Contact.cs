﻿using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Contact: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string Type { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Phone2 { get; set; }

    public string? Role { get; set; }

    public string? SubRole { get; set; }

    public Status Status { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public string FullName { get; private set; } = string.Empty;

    public State? State { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? Zip { get; set; }

    public string? Address { get; set; }

    public ICollection<ContactActivityLog> ContactActivityLogs { get; set; } = new HashSet<ContactActivityLog>();

    public ICollection<Client> Clients { get; set; } = new HashSet<Client>();
}
