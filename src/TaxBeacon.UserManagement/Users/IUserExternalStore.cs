using System.Net.Mail;

namespace TaxBeacon.UserManagement.Users;

public interface IUserExternalStore
{
    /// <summary>
    /// Creates new user identity
    /// </summary>
    /// <param name="mailAddress"></param>
    /// <returns></returns>
    Task<string> CreateUserAsync(MailAddress mailAddress,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);
}
