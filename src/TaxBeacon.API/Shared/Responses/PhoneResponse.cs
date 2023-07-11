namespace TaxBeacon.API.Shared.Responses;

public record PhoneResponse(Guid Id, string Type, string Number, string? Extension);
