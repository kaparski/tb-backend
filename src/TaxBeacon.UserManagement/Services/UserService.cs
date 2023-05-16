﻿using Gridify;
using Gridify.EntityFramework;
using Mapster;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.Common.Converters;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Extensions;
using TaxBeacon.UserManagement.Services.Activities;

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
        _userActivityFactories = userActivityFactories?.ToImmutableDictionary(x => (EventType: x.UserEventType, x.Revision))
                                 ?? ImmutableDictionary<(UserEventType, uint), IUserActivityFactory>.Empty;
    }

    public async Task<OneOf<LoginUserDto, NotFound>> LoginAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _context
            .Users
            .FirstOrDefaultAsync(u => mailAddress.Address == u.Email, cancellationToken);

        if (user is null)
        {
            return new NotFound();
        }

        user.LastLoginDateTimeUtc = _dateTimeService.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) has logged in",
            _dateTimeService.UtcNow,
            user.Id);

        // TODO: make a seeder for roles and use roleId instead of role name 
        return new LoginUserDto(
            user.Id,
            user.FullName,
            await GetUserPermissionsAsync(user.Id, cancellationToken),
            await HasNoTenantRoleAsync(user.Id, "Super admin", cancellationToken));
    }

    public async Task<OneOf<QueryablePaging<UserDto>, NotFound>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var users = await _context
            .Users
            .AsNoTracking()
            .Where(u => u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId))
            .MapToUserDtoWithTenantRoleNames(_context, _currentUserService)
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return gridifyQuery.Page != 1 && gridifyQuery.Page > Math.Ceiling((double)users.Count / gridifyQuery.PageSize)
            ? new NotFound()
            : users;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var users = _currentUserService.TenantId == default
            ? _context
                .Users
                .MapToUserDtoWithNoTenantRoleNames(_context)
            : _context
                .Users
                .MapToUserDtoWithTenantRoleNames(_context, _currentUserService);

        var user = await users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
               ?? throw new NotFoundException(nameof(User), id);

        if (id == _currentUserService.UserId)
        {
            user.Roles = string.Join(", ", (_currentUserService.Roles ?? Enumerable.Empty<string>())
                                            .Concat(_currentUserService.TenantRoles ?? Enumerable.Empty<string>()).Order());
        }
        else if (!string.IsNullOrEmpty(user.Roles))
        {
            user.Roles = string.Join(", ", user.Roles.Split(",").Select(r => r.Trim()).Order());
        }

        return user;
    }

    public async Task<UserDto> GetUserByEmailAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Users
            .AsNoTracking()
            .ProjectToType<UserDto>()
            .FirstOrDefaultAsync(x => x.Email == mailAddress.Address, cancellationToken)
        ?? throw new NotFoundException(nameof(User), mailAddress.Address);

    public async Task<UserDto> UpdateUserStatusAsync(Guid tenantId, Guid id, Status status,
        CancellationToken cancellationToken = default)
    {
        // TODO: Move the same code into separated method
        var user = await _context
                       .Users
                       .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), id);

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
        }

        user.Status = status;

        var now = _dateTimeService.UtcNow;
        var currentUser = await GetUserByIdAsync(_currentUserService.UserId, cancellationToken);

        var userActivityLog = status switch
        {
            Status.Active => new UserActivityLog
            {
                TenantId = tenantId,
                UserId = user.Id,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UserReactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUser.FullName,
                        currentUser.Roles)),
                EventType = UserEventType.UserReactivated
            },
            Status.Deactivated => new UserActivityLog
            {
                TenantId = tenantId,
                UserId = user.Id,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UserDeactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUser.FullName,
                        currentUser.Roles)),
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

    public async Task<UserDto> CreateUserAsync(
        UserDto newUserData,
        CancellationToken cancellationToken = default)
    {
        // TODO: This is a temporary solution for tenants, because we will always keep one tenant in db for now
        var user = newUserData.Adapt<User>();
        var tenant = _context.Tenants.First();
        user.Status = Status.Active;
        user.Id = Guid.NewGuid();

        var userEmail = new MailAddress(newUserData.Email);

        if (!_domainsToSkipExternalStorageUserCreation.Contains(userEmail.Host))
        {
            _ = await _userExternalStore.CreateUserAsync(userEmail,
                newUserData.FirstName,
                newUserData.LastName,
                cancellationToken);
        }

        if (await EmailExistsAsync(user.Email, cancellationToken))
        {
            throw new ConflictException(ConflictExceptionMessages.EmailExistsMessage,
                ConflictExceptionKey.UserEmail);
        }

        user.TenantUsers.Add(new TenantUser
        {
            Tenant = tenant
        });
        await _context.Users.AddAsync(user, cancellationToken);

        var now = _dateTimeService.UtcNow;
        var currentUser = await GetUserByIdAsync(_currentUserService.UserId, cancellationToken);

        await _context.UserActivityLogs.AddAsync(new UserActivityLog
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            Date = _dateTimeService.UtcNow,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new UserCreatedEvent(_currentUserService.UserId,
                    userEmail.Address,
                    now,
                    currentUser.FullName,
                    currentUser.Roles)),
            EventType = UserEventType.UserCreated
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) was created by {@userId}",
            _dateTimeService.UtcNow,
            user.Id,
            _currentUserService.UserId);

        return user.Adapt<UserDto>();
    }

    public async Task<byte[]> ExportUsersAsync(Guid tenantId, FileType fileType,
        CancellationToken cancellationToken)
    {
        tenantId = tenantId != default ? tenantId : (await _context.Tenants.FirstAsync(cancellationToken)).Id;

        var exportUsers = await _context.Users
            .Where(u => u.TenantUsers.Any(tu => tu.TenantId == tenantId))
            .OrderBy(u => u.Email)
            .AsNoTracking()
            .ProjectToType<UserExportModel>()
            .ToListAsync(cancellationToken);

        exportUsers.ForEach(u =>
        {
            u.DeactivationDateTimeView = _dateTimeFormatter.FormatDate(u.DeactivationDateTimeUtc);
            u.ReactivationDateTimeView = _dateTimeFormatter.FormatDate(u.ReactivationDateTimeUtc);
            u.CreatedDateView = _dateTimeFormatter.FormatDate(u.CreatedDateTimeUtc);
            u.LastLoginDateView = _dateTimeFormatter.FormatDate(u.LastLoginDateTimeUtc);
        });

        _logger.LogInformation("{dateTime} - Users export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportUsers);
    }

    public async Task AssignRoleAsync(Guid tenantId, Guid[] roleIds, Guid userId, CancellationToken cancellationToken)
    {
        var existingRoleIds = await _context.TenantUserRoles
            .Where(e => e.UserId == userId && e.TenantId == tenantId)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        var currentUserId = _currentUserService.UserId;
        var removedRoles = _context
            .TenantUserRoles.Where(x => !roleIds.Contains(x.RoleId) && x.UserId == userId && x.TenantId == tenantId);
        _context.TenantUserRoles.RemoveRange(removedRoles);

        var roleIdsToAdd = roleIds.Except(existingRoleIds);

        var tenantUserRoles = roleIdsToAdd.Select(roleId =>
            new TenantUserRole()
            {
                RoleId = roleId,
                UserId = userId,
                TenantId = tenantId
            });
        await _context.TenantUserRoles.AddRangeAsync(tenantUserRoles, cancellationToken);

        var currentUserFullName = (await _context.TenantUsers
                           .Include(x => x.User)
                           .FirstOrDefaultAsync(x => x.UserId == _currentUserService.UserId && x.TenantId == tenantId,
                               cancellationToken))?
                       .User.FullName
                       ?? "";

        var removedRolesString = string.Join(", ", removedRoles
            .Select(x => x.TenantRole.Role.Name));

        var addedRolesString = await _context
            .Roles
            .Where(x => roleIdsToAdd.Contains(x.Id))
            .GroupBy(r => 1, t => t.Name)
            .Select(group => string.Join(", ", group.Select(name => name)))
            .FirstOrDefaultAsync(cancellationToken);

        var currentUserRoles =
            string.Join(", ", await _context
            .TenantUserRoles
            .Where(x => x.TenantId == tenantId && x.UserId == currentUserId)
            .Select(x => x.TenantRole.Role.Name)
            .ToListAsync(cancellationToken));

        if (!string.IsNullOrEmpty(addedRolesString))
        {
            await _context.UserActivityLogs.AddAsync(new UserActivityLog
            {
                TenantId = tenantId,
                UserId = userId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new AssignRolesEvent(
                        addedRolesString,
                        _dateTimeService.UtcNow,
                        currentUserId,
                        currentUserFullName,
                        currentUserRoles
                        )),
                EventType = UserEventType.UserRolesAssign
            }, cancellationToken);
        }

        if (!string.IsNullOrEmpty(removedRolesString))
        {
            await _context.UserActivityLogs.AddAsync(new UserActivityLog
            {
                TenantId = tenantId,
                UserId = userId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UnassignRolesEvent(
                        removedRolesString,
                        _dateTimeService.UtcNow,
                        currentUserId,
                        currentUserFullName,
                        currentUserRoles)
                    ),
                EventType = UserEventType.UserRolesUnassign
            }, cancellationToken);
        }

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
    }

    public async Task<OneOf<UserDto, NotFound>> UpdateUserByIdAsync(Guid tenantId,
        Guid userId,
        UpdateUserDto updateUserDto,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId != default ? tenantId : (await _context.Tenants.FirstAsync(cancellationToken)).Id;

        var user = await _context.Users
            .Where(u => u.Id == userId && u.TenantUsers.Any(tu => tu.TenantId == tenantId))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return new NotFound();
        }

        var previousUserValues = JsonSerializer.Serialize(user.Adapt<UpdateUserDto>());
        user.FirstName = updateUserDto.FirstName;
        user.LegalName = updateUserDto.LegalName;
        user.LastName = updateUserDto.LastName;

        var currentUser = await GetUserByIdAsync(_currentUserService.UserId, cancellationToken);
        var now = _dateTimeService.UtcNow;

        await _context.UserActivityLogs.AddAsync(new UserActivityLog
        {
            TenantId = tenantId,
            UserId = user.Id,
            Date = now,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new UserUpdatedEvent(currentUser.Id,
                    now,
                    currentUser.FullName,
                    currentUser.Roles,
                    previousUserValues,
                    JsonSerializer.Serialize(updateUserDto))),
            EventType = UserEventType.UserUpdated
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var userDto = user.Adapt<UserDto>();
        userDto.Roles = string.Join(", ", await _context
            .TenantUserRoles
            .Where(tu => tu.TenantId == tenantId && tu.UserId == userId)
            .Join(_context.TenantRoles,
                tur => new { tur.TenantId, tur.RoleId },
                tr => new { tr.TenantId, tr.RoleId },
                (tur, tr) => tr.RoleId)
            .Join(_context.Roles, id => id, r => r.Id, (id, r) => r.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken));

        _logger.LogInformation("{dateTime} - User ({createdUserId}) was updated by {@userId}",
            now,
            user.Id,
            _currentUserService.UserId);

        return userDto;
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid userId, uint page = 1,
        uint pageSize = 10, CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        var user = await _context.Users
            .Where(u => u.Id == userId && u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return new NotFound();
        }

        var userActivitiesQuery = _context.UserActivityLogs
            .Where(ua => ua.UserId == userId && ua.TenantId == _currentUserService.TenantId);

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
        CancellationToken cancellationToken = default)
    {
        var (tenantId, isSuperAdmin) = (_currentUserService.TenantId, _currentUserService.IsSuperAdmin);

        if (isSuperAdmin)
        {
            return await GetNoTenantUserPermissionsAsync(userId, cancellationToken);
        }

        return await GetTenantUserPermissionsAsync(_currentUserService.TenantId, userId, cancellationToken);
    }

    public async Task<UserInfo?> GetUserInfoAsync(MailAddress mailAddress, CancellationToken cancellationToken)
    {
        var tenantId = await GetTenantIdAsync(mailAddress, cancellationToken);

        var userQuery = from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId into rolesGrouping
                        from userRole in rolesGrouping.DefaultIfEmpty()
                        join tur in _context.TenantUserRoles on new { UserId = u.Id, TenantId = tenantId } equals new { tur.UserId, tur.TenantId } into tenantRolesGrouping
                        from tenantUserRole in tenantRolesGrouping.DefaultIfEmpty()
                        where u.Email == mailAddress.Address
                        select new { u.Id, u.FullName, Role = userRole.Role.Name, TenantRole = tenantUserRole.TenantRole.Role.Name };

        return (await userQuery.ToListAsync(cancellationToken: cancellationToken))
                                .GroupBy(z => new { z.Id, z.FullName })
                                .Select(g => new UserInfo
                                (
                                    tenantId,
                                    g.Key.Id,
                                    g.Key.FullName,
                                    g.Where(r => !string.IsNullOrEmpty(r.Role)).Select(r => r.Role).Distinct().ToList(),
                                    g.Where(tr => !string.IsNullOrEmpty(tr.TenantRole)).Select(tr => tr.TenantRole).Distinct().ToList()
                                 ))
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

    private Task<Guid> GetTenantIdAsync(MailAddress mail, CancellationToken cancellationToken) =>
        _context.TenantUsers
            .Where(tu => tu.User.Email == mail.Address)
            .Select(tu => tu.TenantId)
            .SingleOrDefaultAsync(cancellationToken);
}
