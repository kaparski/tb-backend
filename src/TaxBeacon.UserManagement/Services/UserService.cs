using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Net.Mail;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Extensions;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services.Activities;
using RolesConstants = TaxBeacon.Common.Roles.Roles;

namespace TaxBeacon.UserManagement.Services;

public class UserService: IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IUserExternalStore _userExternalStore;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IImmutableDictionary<(UserEventType, uint), IUserActivityFactory> _userActivityFactories;

    private readonly IReadOnlyCollection<string> _domainsToSkipExternalStorageUserCreation = new[]
    {
        "ctitaxbeacon.onmicrosoft.com"
    };

    public UserService(
        ILogger<UserService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IUserExternalStore userExternalStore,
        IDateTimeFormatter dateTimeFormatter,
        IEnumerable<IUserActivityFactory> userActivityFactories)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _userExternalStore = userExternalStore;
        _dateTimeFormatter = dateTimeFormatter;
        _userActivityFactories =
            userActivityFactories?.ToImmutableDictionary(x => (EventType: x.UserEventType, x.Revision))
            ?? ImmutableDictionary<(UserEventType, uint), IUserActivityFactory>.Empty;
    }

    public async Task<OneOf<LoginUserDto, NotFound>> LoginAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _context
            .Users
            .SingleOrDefaultAsync(u => mailAddress.Address == u.Email, cancellationToken);

        var tenant = await _context
            .Tenants
            .FirstOrDefaultAsync(t => t.Id == _currentUserService.TenantId, cancellationToken);

        if (user is null)
        {
            return new NotFound();
        }

        var now = _dateTimeService.UtcNow;

        user.LastLoginDateTimeUtc = now;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) has logged in",
            now,
            user.Id);

        return new LoginUserDto(
            user.Id,
            user.FullName,
            await GetUserPermissionsAsync(user.Id, cancellationToken),
            await HasNoTenantRoleAsync(user.Id, RolesConstants.SuperAdmin, cancellationToken),
            tenant?.DivisionEnabled);
    }

    public async Task<QueryablePaging<UserDto>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var users = _currentUserService is { IsUserInTenant: false, IsSuperAdmin: true }
            ? _context.Users
                .AsNoTracking()
                .Where(u => !u.TenantUsers.Any())
                .MapToUserDtoWithNoTenantRoleNames(_context)
            : _context
                .Users
                .AsNoTracking()
                .Where(u => u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId))
                .MapToUserDtoWithTenantRoleNames(_context, _currentUserService);

        return await users.GridifyQueryableAsync(gridifyQuery, null, cancellationToken);
    }

    class UserRoleContainer
    {
        /// <summary>
        /// Concatenation of UserId and TenantId. If user is tenant-free, then it is just UserId.
        /// Needed for optimization purposes.
        /// </summary>
        public string UserIdPlusTenantId { get; set; } = null!;
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }

    public IQueryable<UserDto> QueryUsers()
    {
        var nonTenantUsers = _currentUserService is { IsUserInTenant: false, IsSuperAdmin: true };

        var userRoles = nonTenantUsers ?
            _context.UserRoles
                .Select(ur => new UserRoleContainer
                {
                    UserIdPlusTenantId = ur.UserId.ToString(),
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name
                }) :
            _context.TenantUserRoles
                .Select(tur => new UserRoleContainer
                {
                    UserIdPlusTenantId = tur.UserId.ToString() + tur.TenantId.ToString(),
                    RoleId = tur.RoleId,
                    RoleName = tur.TenantRole.Role.Name
                });

        Guid? tenantId = nonTenantUsers ?
            null :
            _currentUserService.TenantId;

        // Need to use a view here. Main reason is because EF fails to construct a query when
        // an array-like field (Roles in this case) must be both sortable and filterable.
        // Also a view allows to optimize fetching relative fields like Department, JobTitle etc.
        var users = _context.UsersView.Where(u => u.TenantId == tenantId);

        var userDtos = users.GroupJoin(userRoles,
            u => u.UserIdPlusTenantId,
            tur => tur.UserIdPlusTenantId,
            (u, roles) => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Status = u.Status,
                CreatedDateTimeUtc = u.CreatedDateTimeUtc,
                LastLoginDateTimeUtc = u.LastLoginDateTimeUtc,
                DeactivationDateTimeUtc = u.DeactivationDateTimeUtc,
                ReactivationDateTimeUtc = u.ReactivationDateTimeUtc,
                FullName = u.FullName,
                LegalName = u.LegalName,
                DivisionId = u.DivisionId,
                Division = u.Division,
                DepartmentId = u.DepartmentId,
                Department = u.Department,
                JobTitleId = u.JobTitleId,
                JobTitle = u.JobTitle,
                ServiceAreaId = u.ServiceAreaId,
                ServiceArea = u.ServiceArea,
                TeamId = u.TeamId,
                Team = u.Team,
                Roles = u.Roles,
                RoleIds = roles.Select(r => r.RoleId)
            })
        ;

        return userDtos;
    }

    public async Task<OneOf<UserDto, NotFound>> GetUserDetailsByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await QueryUsers().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return new NotFound();
        }

        if (id == _currentUserService.UserId)
        {
            user.Roles = string.Join(", ", _currentUserService.Roles.Concat(_currentUserService.TenantRoles).Order());
        }

        return user;
    }

    public async Task<OneOf<UserDto, NotFound>> GetUserByEmailAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _context
            .Users
            .AsNoTracking()
            .ProjectToType<UserDto>()
            .FirstOrDefaultAsync(x => x.Email == mailAddress.Address, cancellationToken);

        return user is not null ? user : new NotFound();
    }

    public async Task<OneOf<UserDto, NotFound>> UpdateUserStatusAsync(Guid id,
        Status status,
        CancellationToken cancellationToken = default)
    {
        var getUserResult = await GetUserByIdAsync(id, cancellationToken);
        if (!getUserResult.TryPickT0(out var user, out var notFound))
        {
            return notFound;
        }

        switch (status)
        {
            case Status.Deactivated:
                user.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                user.ReactivationDateTimeUtc = null;
                user.Status = Status.Deactivated;
                break;
            case Status.Active:
                user.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                user.DeactivationDateTimeUtc = null;
                user.Status = Status.Active;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        user.Status = status;

        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        var userActivityLog = status switch
        {
            Status.Active => new UserActivityLog
            {
                TenantId = _currentUserService.TenantId,
                UserId = user.Id,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UserReactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = UserEventType.UserReactivated
            },
            Status.Deactivated => new UserActivityLog
            {
                TenantId = _currentUserService.TenantId,
                UserId = user.Id,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UserDeactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = UserEventType.UserDeactivated
            },
            _ => throw new InvalidOperationException()
        };

        await _context.UserActivityLogs.AddAsync(userActivityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) status was changed to {newUserStatus} by {@userId}",
            _dateTimeService.UtcNow,
            user.Id,
            status,
            _currentUserService.UserId);

        return user.Adapt<UserDto>();
    }

    public async Task<OneOf<UserDto, EmailAlreadyExists, InvalidOperation>> CreateUserAsync(
        CreateUserDto newUserData,
        CancellationToken cancellationToken = default)
    {
        var user = newUserData.Adapt<User>();
        user.Status = Status.Active;
        user.Id = Guid.NewGuid();

        var userEmail = new MailAddress(newUserData.Email);

        if (await EmailExistsAsync(user.Email, cancellationToken))
        {
            return new EmailAlreadyExists();
        }

        var validationResult = await ValidateOrganizationUnitsAsync(
            newUserData.DivisionId,
            newUserData.DepartmentId,
            newUserData.ServiceAreaId,
            newUserData.JobTitleId,
            newUserData.TeamId);

        if (!validationResult.TryPickT0(out var ok, out var error))
        {
            return error;
        }

        if (!_domainsToSkipExternalStorageUserCreation.Contains(userEmail.Host))
        {
            _ = await _userExternalStore.CreateUserAsync(userEmail,
                newUserData.FirstName,
                newUserData.LastName,
                cancellationToken);
        }

        if (_currentUserService.TenantId != default)
        {
            user.TenantUsers.Add(new TenantUser { TenantId = _currentUserService.TenantId });
        }

        await _context.Users.AddAsync(user, cancellationToken);

        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        await _context.UserActivityLogs.AddAsync(new UserActivityLog
        {
            TenantId = _currentUserService.TenantId,
            UserId = user.Id,
            Date = now,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new UserCreatedEvent(_currentUserService.UserId,
                    userEmail.Address,
                    now,
                    currentUserInfo.FullName,
                    currentUserInfo.Roles)),
            EventType = UserEventType.UserCreated
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) was created by {@userId}",
            now,
            user.Id,
            _currentUserService.UserId);

        return user.Adapt<UserDto>();
    }

    public async Task<byte[]> ExportUsersAsync(FileType fileType,
        CancellationToken cancellationToken = default)
    {
        byte[] result;
        if (_currentUserService is { IsUserInTenant: false, IsSuperAdmin: true })
        {
            var exportUsers = await _context.Users
                .Where(u => !u.TenantUsers.Any())
                .OrderBy(u => u.Email)
                .ProjectToType<NotTenantUserExportModel>()
                .ToListAsync(cancellationToken);

            exportUsers.ForEach(u =>
            {
                u.DeactivationDateTimeView = _dateTimeFormatter.FormatDate(u.DeactivationDateTimeUtc);
                u.ReactivationDateTimeView = _dateTimeFormatter.FormatDate(u.ReactivationDateTimeUtc);
                u.CreatedDateView = _dateTimeFormatter.FormatDate(u.CreatedDateTimeUtc);
                u.LastLoginDateView = _dateTimeFormatter.FormatDate(u.LastLoginDateTimeUtc);
            });

            result = _listToFileConverters[fileType].Convert(exportUsers);
        }
        else
        {
            var exportUsers = await _context.Users
                .Where(u => u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId))
                .OrderBy(u => u.Email)
                .ProjectToType<TenantUserExportModel>()
                .ToListAsync(cancellationToken);

            exportUsers.ForEach(u =>
            {
                u.DeactivationDateTimeView = _dateTimeFormatter.FormatDate(u.DeactivationDateTimeUtc);
                u.ReactivationDateTimeView = _dateTimeFormatter.FormatDate(u.ReactivationDateTimeUtc);
                u.CreatedDateView = _dateTimeFormatter.FormatDate(u.CreatedDateTimeUtc);
                u.LastLoginDateView = _dateTimeFormatter.FormatDate(u.LastLoginDateTimeUtc);
            });

            result = _listToFileConverters[fileType].Convert(exportUsers);
        }

        _logger.LogInformation("{dateTime} - Users export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return result;
    }

    public async Task<OneOf<Success, NotFound>> ChangeUserRolesAsync(Guid userId,
        Guid[] roleIds,
        CancellationToken cancellationToken = default)
    {
        if (!await IsUserExists(userId, cancellationToken))
        {
            return new NotFound();
        }

        var currentUserInfo = _currentUserService.UserInfo;
        var removedRolesString = await RemoveRolesAsync(userId, roleIds, currentUserInfo, cancellationToken);
        var addedRolesString = await AddRolesAsync(userId, roleIds, currentUserInfo, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(addedRolesString))
        {
            _logger.LogInformation("{dateTime} - User ({userId}) was assigned to {roles} roles by {@userId}",
                _dateTimeService.UtcNow,
                userId,
                addedRolesString,
                _currentUserService.UserId);
        }

        if (!string.IsNullOrEmpty(removedRolesString))
        {
            _logger.LogInformation("{dateTime} - User ({userId}) was unassigned from {roles} roles by {@userId}",
                _dateTimeService.UtcNow,
                userId,
                removedRolesString,
                _currentUserService.UserId);
        }

        return new Success();
    }

    public async Task<OneOf<UserDto, NotFound, InvalidOperation>> UpdateUserByIdAsync(Guid userId,
        UpdateUserDto updateUserDto,
        CancellationToken cancellationToken = default)
    {
        var getUserResult = await GetUserByIdAsync(userId, cancellationToken);

        if (!getUserResult.TryPickT0(out var user, out var notFound))
        {
            return notFound;
        }

        var validationResult = await ValidateOrganizationUnitsAsync(
            updateUserDto.DivisionId,
            updateUserDto.DepartmentId,
            updateUserDto.ServiceAreaId,
            updateUserDto.JobTitleId,
            updateUserDto.TeamId);

        if (!validationResult.TryPickT0(out var ok, out var error))
        {
            return error;
        }

        var previousUserValues = JsonSerializer.Serialize(user.Adapt<UpdateUserDto>());

        updateUserDto.Adapt(user);

        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        await _context.UserActivityLogs.AddAsync(new UserActivityLog
        {
            TenantId = _currentUserService.TenantId,
            UserId = user.Id,
            Date = now,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new UserUpdatedEvent(
                    _currentUserService.UserId,
                    now,
                    currentUserInfo.FullName,
                    currentUserInfo.Roles,
                    previousUserValues,
                    JsonSerializer.Serialize(updateUserDto))),
            EventType = UserEventType.UserUpdated
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({updatedUserId}) was updated by {@userId}",
            now,
            user.Id,
            _currentUserService.UserId);

        var userDto = await GetUserDetailsByIdAsync(userId, cancellationToken);

        return userDto.AsT0;
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid userId, uint page = 1,
        uint pageSize = 10, CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        var getUserResult = await GetUserByIdAsync(userId, cancellationToken);

        if (!getUserResult.TryPickT0(out var _, out var notFound))
        {
            return notFound;
        }

        var userActivitiesQuery = _context.UserActivityLogs
            .Where(ua => ua.UserId == userId);

        var count = await userActivitiesQuery.CountAsync(cancellationToken: cancellationToken);
        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await userActivitiesQuery
            .OrderByDescending(x => x.Date)
            .Skip((int)((page - 1) * pageSize))
            .Take((int)pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _userActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Guid userId,
        CancellationToken cancellationToken = default) =>
        _currentUserService.IsSuperAdmin
            ? await GetNoTenantUserPermissionsAsync(userId, cancellationToken)
            : await GetTenantUserPermissionsAsync(_currentUserService.TenantId, userId, cancellationToken);

    public async Task<UserInfo?> GetUserInfoAsync(MailAddress mailAddress, CancellationToken cancellationToken)
    {
        var tenant = await GetTenantAsync(mailAddress, cancellationToken);
        var tenantId = tenant?.Id ?? Guid.Empty;
        var userQuery = from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId into rolesGrouping
                        from userRole in rolesGrouping.DefaultIfEmpty()
                        join tur in _context.TenantUserRoles on new { UserId = u.Id, TenantId = tenantId } equals new
                        {
                            tur.UserId,
                            tur.TenantId
                        } into tenantRolesGrouping
                        from tenantUserRole in tenantRolesGrouping.DefaultIfEmpty()
                        where u.Email == mailAddress.Address
                        select new
                        {
                            u.Id,
                            u.FullName,
                            Role = userRole.Role.Name,
                            TenantRole = tenantUserRole.TenantRole.Role.Name
                        };

        return (await userQuery.ToListAsync(cancellationToken: cancellationToken))
            .GroupBy(z => new { z.Id, z.FullName })
            .Select(g => new UserInfo
            (
                tenantId,
                tenant?.DivisionEnabled ?? false,
                g.Key.Id,
                g.Key.FullName,
                g.Where(r => !string.IsNullOrEmpty(r.Role)).Select(r => r.Role).Distinct().ToList(),
                g.Where(tr => !string.IsNullOrEmpty(tr.TenantRole)).Select(tr => tr.TenantRole).Distinct().ToList()))
            .SingleOrDefault();
    }

    private async Task<IReadOnlyCollection<string>> GetTenantUserPermissionsAsync(Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantUserRoles
            .AsNoTracking()
            .Where(tur => tur.TenantId == tenantId && tur.UserId == userId)
            .Join(_context.TenantRolePermissions,
                tur => new { tur.TenantId, tur.RoleId },
                trp => new { trp.TenantId, trp.RoleId },
                (tur, trp) => trp.PermissionId)
            .Join(_context.Permissions, id => id, p => p.Id, (id, p) => p.Name)
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyCollection<string>> GetNoTenantUserPermissionsAsync(Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions,
                ur => new { ur.RoleId },
                rp => new { rp.RoleId },
                (ur, rp) => rp.PermissionId)
            .Join(_context.Permissions, id => id, p => p.Id, (id, p) => p.Name)
            .ToListAsync(cancellationToken);

    private async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(x => x.Email == email, cancellationToken);

    private async Task<bool> HasNoTenantRoleAsync(Guid id,
        string roleName,
        CancellationToken cancellationToken = default) =>
        await _context
            .UserRoles
            .AnyAsync(ur => ur.UserId == id && ur.Role.Name == roleName, cancellationToken);

    private Task<Tenant?> GetTenantAsync(MailAddress mail, CancellationToken cancellationToken) =>
        _context.TenantUsers
            .Where(tu => tu.User.Email == mail.Address)
            .Select(tu => tu.Tenant)
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<OneOf<User, NotFound>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = _currentUserService is { IsUserInTenant: false, IsSuperAdmin: true }
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            : await _context.Users.FirstOrDefaultAsync(
                u => u.Id == id && u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId),
                cancellationToken);

        return user is not null ? user : new NotFound();
    }

    private async Task<bool> IsUserExists(Guid id, CancellationToken cancellationToken) =>
        _currentUserService is { IsUserInTenant: false, IsSuperAdmin: true }
            ? await _context.Users.AnyAsync(u => u.Id == id, cancellationToken)
            : await _context.Users.AnyAsync(
                u => u.Id == id && u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId),
                cancellationToken);

    private async Task<string> RemoveRolesAsync(Guid userId,
        Guid[] roleIds,
        (string FullName, string Roles) userInfo,
        CancellationToken cancellationToken)
    {
        string removedRolesString;

        if (_currentUserService is { IsUserInTenant: false, IsSuperAdmin: true })
        {
            var removedRoles = _context
                .UserRoles
                .Where(x => !roleIds.Contains(x.RoleId) && x.UserId == userId);
            _context.UserRoles.RemoveRange(removedRoles);

            removedRolesString = string.Join(", ", removedRoles
                .Select(x => x.Role.Name));
        }
        else
        {
            var removedRoles = _context
                .TenantUserRoles.Where(x =>
                    !roleIds.Contains(x.RoleId) && x.UserId == userId && x.TenantId == _currentUserService.TenantId);
            _context.TenantUserRoles.RemoveRange(removedRoles);

            removedRolesString = string.Join(", ", removedRoles
                .Select(x => x.TenantRole.Role.Name));
        }

        if (!string.IsNullOrEmpty(removedRolesString))
        {
            await _context.UserActivityLogs.AddAsync(new UserActivityLog
            {
                TenantId = _currentUserService.TenantId,
                UserId = userId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UnassignRolesEvent(
                        removedRolesString,
                        _dateTimeService.UtcNow,
                        _currentUserService.UserId,
                        userInfo.FullName,
                        userInfo.Roles)
                ),
                EventType = UserEventType.UserRolesUnassign
            }, cancellationToken);
        }

        return removedRolesString;
    }

    private async Task<string> AddRolesAsync(Guid userId,
        Guid[] roleIds,
        (string FullName, string Roles) userInfo,
        CancellationToken cancellationToken)
    {
        IEnumerable<Guid> roleIdsToAdd;

        if (_currentUserService is { IsUserInTenant: false, IsSuperAdmin: true })
        {
            var existingRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            roleIdsToAdd = roleIds.Except(existingRoleIds).ToList();

            await _context.UserRoles
                .AddRangeAsync(roleIdsToAdd
                    .Select(roleId => new UserRole { RoleId = roleId, UserId = userId }), cancellationToken);
        }
        else
        {
            var existingRoleIds = await _context.TenantUserRoles
                .Where(tur => tur.TenantId == _currentUserService.TenantId && tur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            roleIdsToAdd = roleIds.Except(existingRoleIds).ToList();

            await _context.TenantUserRoles
                .AddRangeAsync(roleIdsToAdd
                    .Select(roleId => new TenantUserRole
                    {
                        RoleId = roleId,
                        UserId = userId,
                        TenantId = _currentUserService.TenantId
                    }), cancellationToken);
        }

        var addedRolesString = string.Join(", ", await _context.Roles
            .Where(r => roleIdsToAdd.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken));

        if (!string.IsNullOrEmpty(addedRolesString))
        {
            await _context.UserActivityLogs.AddAsync(new UserActivityLog
            {
                TenantId = _currentUserService.TenantId,
                UserId = userId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new AssignRolesEvent(
                        addedRolesString,
                        _dateTimeService.UtcNow,
                        _currentUserService.UserId,
                        userInfo.FullName,
                        userInfo.Roles
                    )),
                EventType = UserEventType.UserRolesAssign
            }, cancellationToken);
        }

        return addedRolesString;
    }

    private async Task<OneOf<Success, InvalidOperation>> ValidateOrganizationUnitsAsync(
        Guid? divisionId,
        Guid? departmentId,
        Guid? serviceAreaId,
        Guid? jobTitleId,
        Guid? teamId)
    {
        var tenantId = _currentUserService.TenantId;

        if (divisionId is not null)
        {
            var divisionExists = await _context.Divisions
                .AnyAsync(d => d.Id == divisionId && d.TenantId == tenantId);
            if (!divisionExists)
                return new InvalidOperation($"Division with the ID {divisionId} does not exist.");
        }

        if (departmentId is not null && (divisionId is not null || !_currentUserService.DivisionEnabled))
        {
            var departmentExists = _currentUserService.DivisionEnabled
                ? await _context.Departments
                    .AnyAsync(d => d.Id == departmentId && d.DivisionId == divisionId)
                : await _context.Departments
                    .AnyAsync(d => d.Id == departmentId && d.TenantId == _currentUserService.TenantId);

            if (!departmentExists)
                return new InvalidOperation($"Department with the ID {departmentId} does not exist.");
        }

        if (serviceAreaId is not null && departmentId is not null)
        {
            var serviceAreaExists = await _context.ServiceAreas
                        .AnyAsync(d => d.Id == serviceAreaId && d.DepartmentId == departmentId);
            if (!serviceAreaExists)
                return new InvalidOperation($"Service area with the ID {serviceAreaId} does not exist.");
        }

        if (jobTitleId is not null && departmentId is not null)
        {
            var jobTitleExists = await _context.JobTitles
                        .AnyAsync(d => d.Id == jobTitleId && d.DepartmentId == departmentId);
            if (!jobTitleExists)
                return new InvalidOperation($"Job title with the ID {jobTitleId} does not exist.");
        }

        if (teamId is not null)
        {
            var teamExists = await _context.Teams
                .AnyAsync(d => d.Id == teamId && d.TenantId == tenantId);
            if (!teamExists)
                return new InvalidOperation($"Team with the ID {teamId} does not exist.");
        }

        return new Success();
    }
}
