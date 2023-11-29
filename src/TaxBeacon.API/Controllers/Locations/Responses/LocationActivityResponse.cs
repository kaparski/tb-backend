namespace TaxBeacon.API.Controllers.Locations.Responses;

public record LocationActivityResponse(uint Count, IEnumerable<LocationActivityItemResponse> Query);

public record LocationActivityItemResponse(string Message, DateTime Date, string FullName);