using Gridify;
using TaxBeacon.UserManagement.Models;
using System.Net.Mail;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    Task<QueryablePaging<UserDto>> GetUsersAsync(GridifyQuery gridifyQuery, CancellationToken cancellationToken);

    Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);

    Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserDto> GetUserByEmailAsync(MailAddress mailAddress, CancellationToken cancellationToken);

    Task<UserDto> UpdateUserStatusAsync(Guid id, UserStatus userStatus, CancellationToken cancellationToken);
}
