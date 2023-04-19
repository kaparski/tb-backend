namespace TaxBeacon.API.Controllers.Tenants.Responses;

public record TenantActivityHistoryResponse(uint Count, IEnumerable<TenantActivityLogResponse> Query);

public record TenantActivityLogResponse(string Message, DateTime Date, string FullName);