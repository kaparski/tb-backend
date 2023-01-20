namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    Task LoginAsync(string email, CancellationToken cancellationToken = default);
}
