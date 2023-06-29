namespace TaxBeacon.Administration.Users.Models;

public record LoginUserDto(Guid UserId, string FullName, IReadOnlyCollection<string> Permissions, bool IsSuperAdmin, bool? DivisionsEnabled);
