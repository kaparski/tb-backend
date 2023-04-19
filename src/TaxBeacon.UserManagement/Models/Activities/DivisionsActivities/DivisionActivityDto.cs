namespace TaxBeacon.UserManagement.Models.Activities.DivisionsActivities;

public record DivisionActivityDto(uint Count, IEnumerable<DivisionActivityItemDto> Query);

public record DivisionActivityItemDto(string Message, DateTime Date, string FullName);
