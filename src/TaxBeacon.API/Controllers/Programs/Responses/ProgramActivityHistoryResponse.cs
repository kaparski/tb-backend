namespace TaxBeacon.API.Controllers.Programs.Responses;

public record ProgramActivityHistoryResponse(uint Count, IEnumerable<ProgramActivityLogResponse> Query);

public record ProgramActivityLogResponse(string Message, DateTime Date, string FullName);
