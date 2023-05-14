namespace TaxBeacon.API.Controllers.JobTitles.Responses;

public record JobTitleActivityHistoryResponse(uint Count, IEnumerable<JobTitleActivityLogResponse> Query);

public record JobTitleActivityLogResponse(string Message, DateTime Date, string FullName);
