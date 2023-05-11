namespace TaxBeacon.UserManagement.Models
{
    public sealed class UserInfo
    {
        public UserInfo(Guid tenantId, Guid userId, string fullName, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> tenantRoles)
        {
            TenantId = tenantId;
            Id = userId;
            FullName = fullName;
            Roles = roles;
            TenantRoles = tenantRoles;
        }

        public Guid Id { get; }

        public string FullName { get; }

        public Guid TenantId { get; }

        public IReadOnlyCollection<string> Roles { get; } = Enumerable.Empty<string>().ToList();

        public IReadOnlyCollection<string> TenantRoles { get; } = Enumerable.Empty<string>().ToList();
    }
}
