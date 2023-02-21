using Gridify;
using TaxBeacon.UserManagement.Models;
using System.Net.Mail;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    Task<QueryablePaging<UserDto>> GetUsersAsync(GridifyQuery gridifyQuery, CancellationToken cancellationToken);
    Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    HashSet<PermissionEnum> GetUserPermissionsByEmail(string email);
    Task<User> CreateUserAsync(
        User user,
        CancellationToken cancellationToken = default);
}
