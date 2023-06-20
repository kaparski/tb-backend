namespace TaxBeacon.Common.Models;

public record ActivityDto(uint Count, IEnumerable<ActivityItemDto> Query);

public record ActivityItemDto(string Message, DateTime Date, string FullName);
