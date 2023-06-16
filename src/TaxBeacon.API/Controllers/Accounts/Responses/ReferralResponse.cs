using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record ReferralResponse(ReferralState State, Status Status);
