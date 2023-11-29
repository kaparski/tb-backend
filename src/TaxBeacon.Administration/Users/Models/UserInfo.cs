using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.Users.Models;

public record UserInfo(
    Guid TenantId,
    Guid Id,
    string FullName,
    Status Status,
    bool DivisionEnabled,
    string? IdpExternalId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> TenantRoles);
