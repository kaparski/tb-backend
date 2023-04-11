namespace TaxBeacon.UserManagement.Models;

public record LoginUserDto(Guid UerId, string FullName, IReadOnlyCollection<string> Permissions, bool IsSuperAdmin);
