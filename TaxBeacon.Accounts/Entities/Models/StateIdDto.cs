﻿using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Entities.Models;
public record StateIdDto
{
    public Guid Id { get; init; }

    public Guid EntityId { get; init; }

    public Guid TenantId { get; init; }

    public State State { get; init; }

    public string StateIdType { get; init; } = null!;

    public string StateIdCode { get; init; } = null!;

    public string? LocalJurisdiction { get; init; }
}