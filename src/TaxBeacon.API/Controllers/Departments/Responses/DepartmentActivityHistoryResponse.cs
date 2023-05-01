namespace TaxBeacon.API.Controllers.Departments.Responses;

public record DepartmentActivityHistoryResponse(uint Count, IEnumerable<DepartmentActivityLogResponse> Query);

public record DepartmentActivityLogResponse(string Message, DateTime Date, string FullName);
