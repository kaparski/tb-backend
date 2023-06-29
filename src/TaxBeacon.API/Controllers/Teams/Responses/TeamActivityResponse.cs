namespace TaxBeacon.API.Controllers.Teams.Responses;

public record TeamActivityResponse(uint Count, IEnumerable<TeamActivityItemResponse> Query);

public record TeamActivityItemResponse(string Message, DateTime Date, string FullName);