namespace TaxBeacon.API.Exceptions;

public record InvalidOperationResponse(string Message, string? ParamName = null);
