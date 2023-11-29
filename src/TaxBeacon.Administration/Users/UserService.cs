using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Net.Mail;
using System.Text.Json;
using TaxBeacon.Administration.Tenants.Models;
using TaxBeacon.Administration.Users.Activities.Factories;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Administration.Users.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Options;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.Email;
using TaxBeacon.Email.Messages;
using RolesConstants = TaxBeacon.Common.Constants.Roles;

namespace TaxBeacon.Administration.Users;

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
    private readonly CreateUserOptions _createUserOptions;
    private readonly IEmailSender _emailSender;

    public UserService(
        ILogger<UserService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IUserExternalStore userExternalStore,
        IDateTimeFormatter dateTimeFormatter,
        IEnumerable<IUserActivityFactory> userActivityFactories,
        IOptionsSnapshot<CreateUserOptions> createUserOptionsSnapshot,
        IEmailSender emailSender)
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
        _createUserOptions = createUserOptionsSnapshot.Value;
        _emailSender = emailSender;
    }

    public async Task<OneOf<LoginUserDto, TenantDto[], NotFound>> LoginAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default)
    {
        var userTenants = await _context.TenantUsers
            .Where(tu => tu.User.Email == mailAddress.Address)
            .Select(tu => tu.Tenant)
            .ToArrayAsync(cancellationToken);

        Tenant? tenant;

        if (userTenants.Length > 1)
        {
            // Trying to use the hint from user, in which tenant they want to login to
            tenant = userTenants.SingleOrDefault(t => t.Id == _currentUserService.TenantIdFromHeaders);

            if (tenant == null)
            {
                // Returning the list of user's tenants, so they can choose which one to login into
                return userTenants.Adapt<TenantDto[]>();
            }
        }
        else
        {
            tenant = userTenants.SingleOrDefault();
        }

        var user = tenant == null
            ? await _context.Users
                .Where(u => u.Email == mailAddress.Address)
                .SingleOrDefaultAsync(cancellationToken)
            : await _context.TenantUsers
                .Where(tu => tu.TenantId == tenant.Id && tu.User.Email == mailAddress.Address)
                .Select(tu => tu.User)
                .SingleOrDefaultAsync(cancellationToken);

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
            await GetUserPermissionsAsync(user.Id, tenant?.Id ?? Guid.Empty, cancellationToken),
            await HasNoTenantRoleAsync(user.Id, RolesConstants.SuperAdmin, cancellationToken),
            tenant?.DivisionEnabled,
            tenant?.Id,
            tenant?.Name);
    }

    public IQueryable<UserDto> QueryUsers()
    {
        // Need to use views here. Main reason is because EF fails to construct a query when
        // an array-like field (Roles in this case) must be both sortable and filterable.
        // Also a view allows to optimize fetching relative fields like Department, JobTitle etc.
        var nonTenantUsers = _currentUserService is { IsUserInTenant: false, IsSuperAdmin: true };

        return nonTenantUsers ? QueryNotTenantUsers() : QueryTenantUsers();
    }

    public async Task<OneOf<UserDto, NotFound>> GetUserDetailsByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await QueryUsers().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return user is null ? new NotFound() : user;
    }

    public async Task<OneOf<UserDto, NotFound>> GetUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var userQuery =
            _currentUserService is { IsUserInTenant: true, IsSuperAdmin: true } || !_currentUserService.IsUserInTenant
                ? QueryNotTenantUsers()
                : QueryTenantUsers();

        var user = await userQuery.SingleOrDefaultAsync(x => x.Id == _currentUserService.UserId, cancellationToken);

        return user is null ? new NotFound() : user;
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

        _logger.LogInformation("{dateTime} - User ({createdUserId}) status was changed to {newUserStatus} by {userId}",
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

        if (await EmailExistsAsync(user.Email, _currentUserService.TenantId, cancellationToken))
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

        // Determining whether it should be a 'local' B2C user or a user that comes from an external AAD tenant.
        // Trying to match 'Host' part of the email with our CreateUser/KnownAadTenants config section.
        var externalAadIssuerUrl = _createUserOptions
            .KnownAadTenants
            .FirstOrDefault(t => t.DomainName.Equals(userEmail.Host, StringComparison.OrdinalIgnoreCase))?
            .IssuerUrl;

        if (string.IsNullOrEmpty(externalAadIssuerUrl) && !string.IsNullOrEmpty(newUserData.ExternalAadUserObjectId))
        {
            return new InvalidOperation(
                "This email address is not associated with any external AAD tenant. Can only create a local B2C user for it, but in that case user's external ObjectId should not be specified.");
        }

        var (aadB2CObjectId, userType, password) = await _userExternalStore.CreateUserAsync(userEmail,
            newUserData.FirstName,
            newUserData.LastName,
            externalAadIssuerUrl,
            newUserData.ExternalAadUserObjectId,
            cancellationToken);

        // Storing this B2C user's ObjectId in a separate field in our Users table.
        // This value will then be used as part of the login process.
        user.IdpExternalId = aadB2CObjectId;

        // Also storing user type, just for the reference and (potentially) future use.
        user.UserType = userType;

        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        if (userType == UserType.LocalB2C)
        {
            // Only sending the password email if the user was indeed just created as a local B2C user
            await _emailSender.SendAsync(EmailType.UserCreated,
                _createUserOptions.Recipients,
                new UserCreatedMessage(userEmail.Address, password));

            await _context.UserActivityLogs.AddAsync(new UserActivityLog
            {
                TenantId = _currentUserService.TenantId,
                UserId = user.Id,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new CredentialsSentEvent(_currentUserService.UserId,
                        userEmail.Address,
                        now,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = UserEventType.CredentialSent
            }, cancellationToken);
        }

        if (_currentUserService.TenantId != default)
        {
            user.TenantUsers.Add(new TenantUser { TenantId = _currentUserService.TenantId });
        }

        await _context.Users.AddAsync(user, cancellationToken);

        now = _dateTimeService.UtcNow;

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

        _logger.LogInformation("{dateTime} - User ({createdUserId}) was created by {userId}",
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
                u.DeactivationDateTimeView = _dateTimeFormatter.FormatDateTime(u.DeactivationDateTimeUtc);
                u.ReactivationDateTimeView = _dateTimeFormatter.FormatDateTime(u.ReactivationDateTimeUtc);
                u.CreatedDateView = _dateTimeFormatter.FormatDateTime(u.CreatedDateTimeUtc);
                u.LastLoginDateView = _dateTimeFormatter.FormatDateTime(u.LastLoginDateTimeUtc);
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
                u.DeactivationDateTimeView = _dateTimeFormatter.FormatDateTime(u.DeactivationDateTimeUtc);
                u.ReactivationDateTimeView = _dateTimeFormatter.FormatDateTime(u.ReactivationDateTimeUtc);
                u.CreatedDateView = _dateTimeFormatter.FormatDateTime(u.CreatedDateTimeUtc);
                u.LastLoginDateView = _dateTimeFormatter.FormatDateTime(u.LastLoginDateTimeUtc);
            });

            result = _listToFileConverters[fileType].Convert(exportUsers);
        }

        _logger.LogInformation("{dateTime} - Users export was executed by {userId}",
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
            _logger.LogInformation("{dateTime} - User ({userId}) was assigned to {roles} roles by {userId}",
                _dateTimeService.UtcNow,
                userId,
                addedRolesString,
                _currentUserService.UserId);
        }

        if (!string.IsNullOrEmpty(removedRolesString))
        {
            _logger.LogInformation("{dateTime} - User ({userId}) was unassigned from {roles} roles by {userId}",
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

        _logger.LogInformation("{dateTime} - User ({updatedUserId}) was updated by {userId}",
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

    public async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Guid userId, Guid tenantId = default,
        CancellationToken cancellationToken = default) =>
        _currentUserService.IsSuperAdmin || tenantId == default
            ? await GetNoTenantUserPermissionsAsync(userId, cancellationToken)
            : await GetTenantUserPermissionsAsync(tenantId, userId, cancellationToken);

    public async Task<UserInfo?> GetUserInfoAsync(MailAddress mailAddress, CancellationToken cancellationToken)
    {
        var userTenants = await _context.TenantUsers
            .Where(tu => tu.User.Email == mailAddress.Address)
            .Select(tu => tu.Tenant)
            .ToArrayAsync(cancellationToken);

        Tenant? tenant;

        if (userTenants.Length > 1)
        {
            // Trying to use the hint from user, in which tenant they want to login to
            tenant = userTenants.SingleOrDefault(t => t.Id == _currentUserService.TenantIdFromHeaders);
        }
        else
        {
            tenant = userTenants.SingleOrDefault();
        }

        return tenant == null
            ? _context.Users
                .Where(u => u.Email == mailAddress.Address)
                .Select(u => new UserInfo(
                    Guid.Empty,
                    u.Id,
                    u.FullName,
                    u.Status,
                    false,
                    u.IdpExternalId,
                    u.UserRoles.Select(ur => ur.Role.Name).ToArray(),
                    Array.Empty<string>()
                ))
                .SingleOrDefault()
            : _context.TenantUsers
                .Where(tu => tu.TenantId == tenant.Id && tu.User.Email == mailAddress.Address)
                .Select(tu => new UserInfo(
                    tenant.Id,
                    tu.User.Id,
                    tu.User.FullName,
                    tu.User.Status,
                    tu.Tenant.DivisionEnabled,
                    tu.User.IdpExternalId,
                    Array.Empty<string>(),
                    tu.TenantUserRoles.Select(tur => tur.TenantRole.Role.Name).ToArray()
                ))
                .SingleOrDefault();
    }

    public Task<bool> UserExistsInTenantAsync(Guid userId, Guid tenantId,
        CancellationToken cancellationToken = default) =>
        _context.TenantUsers.AnyAsync(tu => tu.TenantId == tenantId && tu.UserId == userId, cancellationToken);

    public async Task SetIdpExternalIdAsync(MailAddress mailAddress, string idpExternalId)
    {
        var user = await _context.Users.SingleAsync(u => u.Email == mailAddress.Address);
        user.IdpExternalId = idpExternalId;
        await _context.SaveChangesAsync();
    }

    private IQueryable<UserDto> QueryNotTenantUsers()
    {
        var userRoles = _context.UserRoles
            .Select(ur => new { UserId = ur.UserId, RoleId = ur.RoleId, RoleName = ur.Role.Name });

        var userDtos = _context.UsersView.GroupJoin(userRoles,
            u => u.Id,
            ur => ur.UserId,
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
                LastModifiedDateTimeUtc = u.LastModifiedDateTimeUtc,
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
                RoleIds = roles.Select(r => r.RoleId),
                RoleNames = roles.Select(r => r.RoleName),
            });

        return userDtos;
    }

    private IQueryable<UserDto> QueryTenantUsers()
    {
        var tenantId = _currentUserService.TenantId;

        var users = _context.TenantUsersView.Where(u => u.TenantId == tenantId);

        var userRoles = _context.TenantUserRoles
            .Select(tur => new
            {
                UserIdPlusTenantId = tur.UserId.ToString() + tur.TenantId.ToString(),
                RoleId = tur.RoleId,
                RoleName = tur.TenantRole.Role.Name
            });

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
                LastModifiedDateTimeUtc = u.LastModifiedDateTimeUtc,
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
                RoleIds = roles.Select(r => r.RoleId),
                RoleNames = roles.Select(r => r.RoleName),
            });

        return userDtos;
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

    private async Task<bool> EmailExistsAsync(string email, Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == default)
        {
            // Checking among non-tenant users only
            return await _context.Users.AnyAsync(x => x.Email == email && !x.TenantUsers.Any(), cancellationToken);
        }
        else
        {
            // Checking among users belonging to this particular tenant
            return await _context.TenantUsers.AnyAsync(tu => tu.TenantId == tenantId && tu.User.Email == email,
                cancellationToken);
        }
    }

    private async Task<bool> HasNoTenantRoleAsync(Guid id,
        string roleName,
        CancellationToken cancellationToken = default) =>
        await _context
            .UserRoles
            .AnyAsync(ur => ur.UserId == id && ur.Role.Name == roleName, cancellationToken);

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

        if (divisionId is not null && departmentId is not null && _currentUserService.DivisionEnabled)
        {
            var department = await _context.Departments
                .SingleOrDefaultAsync(d => d.Id == departmentId && d.TenantId == tenantId);
            if (department is not null && department.DivisionId != divisionId)
                return new InvalidOperation($"Division {divisionId} and Department {departmentId} do not match.",
                    "divisionAndDepartment");
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
