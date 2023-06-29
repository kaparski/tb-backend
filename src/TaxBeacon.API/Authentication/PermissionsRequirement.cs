using Microsoft.AspNetCore.Authorization;

namespace TaxBeacon.API.Authentication;

public sealed class PermissionsRequirement: IAuthorizationRequirement
{
    public IReadOnlyCollection<string> Permissions { get; }

    public PermissionsRequirement(string permissions) => Permissions = permissions.Split(";").AsReadOnly();
}