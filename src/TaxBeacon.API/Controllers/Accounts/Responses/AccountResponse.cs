﻿using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record AccountResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? City { get; init; }

    public State? State { get; init; }

    public string AccountType { get; init; } = null!;

    public string? ClientState { get; init; }

    public Status? ClientStatus { get; init; }

    public string? ReferralState { get; init; }

    public Status? ReferralStatus { get; init; }
}
