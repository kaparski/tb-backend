﻿using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record ClientDto
{
    public string State { get; init; } = null!;
    public Status Status { get; init; } = Status.Active;
    public decimal? AnnualRevenue { get; init; }
    public int? FoundationYear { get; init; }
    public int? EmployeeCount { get; init; }
    public DateTime? DeactivationDateTimeUtc { get; init; }
    public DateTime? ReactivationDateTimeUtc { get; init; }
    public DateTime CreatedDateTimeUtc { get; init; }
    public ContactDto? PrimaryContact { get; init; } = null!;
    public ICollection<ClientManagerDto> Managers { get; init; } = new List<ClientManagerDto>();
}
