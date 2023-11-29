namespace TaxBeacon.Common.Errors;

public record InvalidOperation(string Message, string? ParamName = null);
