namespace TaxBeacon.UserManagement.Models;

public record LoginUserDto(Guid UserId, string FullName, IReadOnlyCollection<string> Permissions, bool IsSuperAdmin, bool? DivisionsEnabled);
