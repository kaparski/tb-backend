namespace TaxBeacon.API.Controllers.Programs.Response;

public record ProgramActivityHistoryResponse(uint Count, IEnumerable<ProgramActivityLogResponse> Query);

public record ProgramActivityLogResponse(string Message, DateTime Date, string FullName);