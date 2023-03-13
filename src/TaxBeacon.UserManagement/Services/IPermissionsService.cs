namespace TaxBeacon.UserManagement.Services
{
    public interface IPermissionsService
    {
        Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid tenantId, Guid userId);
    }
}
