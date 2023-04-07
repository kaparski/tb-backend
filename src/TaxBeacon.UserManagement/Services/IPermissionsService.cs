namespace TaxBeacon.UserManagement.Services
{
    public interface IPermissionsService
    {
        Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId,
            CancellationToken cancellationToken = default);
    }
}
