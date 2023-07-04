namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record AccountActivityHistoryResponse(uint Count, IEnumerable<AccountActivityLogResponse> Query);

public record AccountActivityLogResponse(string Message, DateTime Date, string FullName);
