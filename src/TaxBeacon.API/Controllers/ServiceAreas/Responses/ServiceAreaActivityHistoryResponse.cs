namespace TaxBeacon.API.Controllers.ServiceAreas.Responses;

public record ServiceAreaActivityHistoryResponse(uint Count, IEnumerable<ServiceAreaActivityLogResponse> Query);

public record ServiceAreaActivityLogResponse(string Message, DateTime Date, string FullName);
