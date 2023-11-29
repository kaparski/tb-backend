using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Shared.Responses;

public record PhoneResponse(Guid Id, PhoneType Type, string Number, string? Extension, DateTime CreatedDateTimeUtc);
