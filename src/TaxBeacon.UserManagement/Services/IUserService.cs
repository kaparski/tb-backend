using System.Net.Mail;

namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);
}
