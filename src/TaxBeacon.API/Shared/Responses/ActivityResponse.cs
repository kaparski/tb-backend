namespace TaxBeacon.API.Shared.Responses;

public record ActivityResponse(uint Count, IEnumerable<ActivityItemResponse> Query);

public record ActivityItemResponse(string Message, DateTime Date, string FullName);
