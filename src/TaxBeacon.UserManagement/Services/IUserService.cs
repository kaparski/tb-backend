using Gridify;
using System.Net;
using OneOf;
using OneOf.Types;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    Task<OneOf<QueryablePaging<UserDto>, NotFound>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);

    Task<UserDetailsDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDto> GetUserByEmailAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateUserStatusAsync(Guid id, UserStatus userStatus, CancellationToken cancellationToken = default);

    Task<UserDto> CreateUserAsync(
        UserDto user,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportUsersAsync(Guid tenantId, FileType fileType, string ianaTimeZone, CancellationToken cancellationToken);

    Task<Guid> GetTenantIdAsync(Guid userId);

    Task AssignRoleAsync(Guid[] roleIds, Guid userId, CancellationToken cancellationToken);
}
