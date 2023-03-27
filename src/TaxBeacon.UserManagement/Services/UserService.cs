using Gridify;
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
using System.Net;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.Common.Enums.Activities;

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

    private readonly IReadOnlyCollection<string> _domainsToSkipExternalStorageUserCreation = new string[]
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
        IDateTimeFormatter dateTimeFormatter)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _userExternalStore = userExternalStore;
        _dateTimeFormatter = dateTimeFormatter;
    }

    public async Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default)
    {
        var user = await _context
            .Users
            .FirstOrDefaultAsync(u => mailAddress.Address == u.Email, cancellationToken);

        if (user is null)
        {
            await CreateUserAsync(
                new UserDto
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = mailAddress.Address,
                    LastLoginDateTimeUtc = _dateTimeService.UtcNow,
                }, cancellationToken);
        }
        else
        {
            user.LastLoginDateTimeUtc = _dateTimeService.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<OneOf<QueryablePaging<UserDto>, NotFound>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var users = await _context
            .Users
            .ProjectToType<UserDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return gridifyQuery.Page != 1 && gridifyQuery.Page > Math.Ceiling((double)users.Count / gridifyQuery.PageSize)
            ? new NotFound()
            : users;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context
            .Users
            .ProjectToType<UserDto>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(User), id);

    public async Task<UserDto> GetUserByEmailAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Users
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
                EventType = EventType.UserReactivated
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
                EventType = EventType.UserDeactivated
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
            EventType = EventType.UserCreated
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
            .ProjectToType<UserExportModel>()
            .AsNoTracking()
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

    public Task<Guid> GetTenantIdAsync(Guid userId) =>
        _context.TenantUsers
            .Where(tu => tu.UserId == userId)
            .Select(tu => tu.TenantId)
            .FirstOrDefaultAsync();

    public async Task AssignRoleAsync(Guid[] roleIds, Guid userId, CancellationToken cancellationToken)
    {
        var existingRoles = await _context.TenantUserRoles
            .Where(e => e.UserId == userId)
            .Select(x => x.TenantRole.Role)
            .ProjectToType<RoleActivityDto>()
            .ToListAsync(cancellationToken);

        _context.TenantUserRoles.RemoveRange(_context
            .TenantUserRoles.Where(x => !roleIds.Contains(x.RoleId) && x.UserId == userId));

        var rolesString = await _context
            .TenantUserRoles
            .Where(x => x.UserId == userId)
            .GroupBy(r => 1, t => t.TenantRole.Role.Name)
            .Select(group => string.Join(", ", group.Select(name => name)))
            .FirstOrDefaultAsync(cancellationToken);

        var tenant = await _context.Tenants.FirstAsync(cancellationToken);
        var roleIdsToAdd = roleIds.Except(existingRoles.Select(x => x.Id));
        foreach (var roleId in roleIdsToAdd)
        {
            _context
                .TenantUserRoles.Add(new TenantUserRole()
                {
                    RoleId = roleId,
                    UserId = userId,
                    TenantId = tenant.Id
                });
        }

        var fullName = (await _context.Users
                .FirstOrDefaultAsync(x => x.Id == _currentUserService.UserId, cancellationToken))?
                .FullName ?? "";
        var newRoles = await _context.Roles
            .Where(x => roleIds.Contains(x.Id))
            .ProjectToType<RoleActivityDto>()
            .ToListAsync(cancellationToken);

        await _context.UserActivityLogs.AddAsync(new UserActivityLog
        {
            TenantId = tenant.Id,
            UserId = userId,
            Date = _dateTimeService.UtcNow,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new AssignRolesEvent(
                        rolesString ?? "",
                        _currentUserService.UserId,
                        fullName,
                        existingRoles,
                        newRoles)),
            EventType = EventType.UserRolesAssign
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{dateTime} - User ({userId}) was assigned to {roles} roles by {@userId}",
            _dateTimeService.UtcNow,
            userId,
            rolesString,
            _currentUserService.UserId);
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
            EventType = EventType.UserUpdated
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

    private async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(x => x.Email == email, cancellationToken);

    private async Task<IEnumerable<string>> GetUserRolesAsync(Guid tenantId, Guid userId) =>
        await _context.TenantUserRoles.Where(ur => ur.TenantId == tenantId && ur.UserId == userId)
            .Select(ur => ur.TenantRole.Role.Name)
            .ToListAsync();
}
