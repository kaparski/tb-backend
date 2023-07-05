namespace TaxBeacon.API.Controllers.Divisions.Responses;

public record DivisionActivityResponse(uint Count, IEnumerable<DivisionActivityItemResponse> Query);

public record DivisionActivityItemResponse(string Message, DateTime Date, string FullName);
