using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record AccountResponse(
    Guid Id,
    string Name,
    string City,
    State State,
    string AccountType
// ClientResponse? Client,
// ReferralResponse? Referral
);
