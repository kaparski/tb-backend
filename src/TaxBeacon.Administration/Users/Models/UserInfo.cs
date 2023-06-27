namespace TaxBeacon.Administration.Users.Models;

public sealed class UserInfo
{
    public UserInfo(Guid tenantId, bool divisionEnabled, Guid userId, string fullName, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> tenantRoles)
    {
        TenantId = tenantId;
        Id = userId;
        FullName = fullName;
        Roles = roles;
        TenantRoles = tenantRoles;
        DivisionEnabled = divisionEnabled;
    }

    public Guid Id { get; }

    public string FullName { get; }

    public Guid TenantId { get; }

    public bool DivisionEnabled { get; }

    public IReadOnlyCollection<string> Roles { get; } = Enumerable.Empty<string>().ToList();

    public IReadOnlyCollection<string> TenantRoles { get; } = Enumerable.Empty<string>().ToList();
}