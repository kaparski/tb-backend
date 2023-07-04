using Gridify;
using OneOf;
using OneOf.Types;
using System.Net.Mail;
using TaxBeacon.Administration.Users.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Users;

public interface IUserService
{
    Task<QueryablePaging<UserDto>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    IQueryable<UserDto> QueryUsers();

    Task<OneOf<LoginUserDto, NotFound>> LoginAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserDto, NotFound>> GetUserDetailsByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserDto, NotFound>> GetUserByEmailAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserDto, NotFound>> UpdateUserStatusAsync(Guid id,
        Status status,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserDto, EmailAlreadyExists, InvalidOperation>> CreateUserAsync(
        CreateUserDto user,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportUsersAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> ChangeUserRolesAsync(Guid userId,
        Guid[] roleIds,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserDto, NotFound, InvalidOperation>> UpdateUserByIdAsync(Guid userId,
        UpdateUserDto updateUserDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid userId,
        uint page = 1,
        uint pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Guid userId,
        Guid tenantId = default,
        CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);
}
