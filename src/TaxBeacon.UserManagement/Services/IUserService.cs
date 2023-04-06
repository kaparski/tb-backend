using Gridify;
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

    Task<UserDto> LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);

    Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDto> GetUserByEmailAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateUserStatusAsync(Guid tenantId, Guid id, Status userStatus,
        CancellationToken cancellationToken = default);

    Task<UserDto> CreateUserAsync(
        UserDto user,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportUsersAsync(Guid tenantId, FileType fileType, CancellationToken cancellationToken);

    Task<Guid> GetTenantIdAsync(Guid userId);

    Task AssignRoleAsync(Guid tenantId, Guid[] roleIds, Guid userId, CancellationToken cancellationToken);

    Task<OneOf<UserDto, NotFound>> UpdateUserByIdAsync(Guid tenantId, Guid userId, UpdateUserDto updateUserDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserActivityDto, NotFound>> GetActivitiesAsync(Guid userId, uint page = 1, uint pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<TenantDto>, NotFound>> GetTenantsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportTenantsAsync(FileType fileType, CancellationToken cancellationToken);
}
