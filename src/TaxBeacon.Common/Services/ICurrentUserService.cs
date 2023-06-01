namespace TaxBeacon.Common.Services;

public interface ICurrentUserService
{
    public Guid UserId { get; }

    public Guid TenantId { get; }

    (string FullName, string Roles) UserInfo { get; }

    bool IsSuperAdmin { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> TenantRoles { get; }

    bool IsUserInTenant { get; }

    bool DivisionEnabled { get; }
}
