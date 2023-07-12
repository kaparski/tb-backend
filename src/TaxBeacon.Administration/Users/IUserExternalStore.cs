using System.Net.Mail;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.Users;

public interface IUserExternalStore
{
    /// <summary>
    /// Creates new user identity
    /// </summary>
    /// <param name="mailAddress"></param>
    /// <returns></returns>
    Task<(string aadB2CObjectId, UserType userType, string password)> CreateUserAsync(MailAddress mailAddress,
        string firstName,
        string lastName,
        string? externalAadTenantIssuerUrl,
        string? externalAadUserObjectId,
        CancellationToken cancellationToken = default);
}
