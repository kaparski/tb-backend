namespace TaxBeacon.API.Controllers.Entities.Responses;

public record EntityActivityResponse(uint Count, IEnumerable<EntityActivityItemResponse> Query);

public record EntityActivityItemResponse(string Message, DateTime Date, string FullName);
