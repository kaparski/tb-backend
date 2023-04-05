namespace TaxBeacon.UserManagement.Models
{
    public record UserActivityDto(uint Count, IEnumerable<UserActivityItemDto> Query);

    public record UserActivityItemDto(string Message, DateTime Date, string FullName);
}
