using Gridify;
using Gridify.EntityFramework;
using Mapster;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Extensions;
using System.Collections.Immutable;

namespace TaxBeacon.UserManagement.Services;

public class UserService: IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IUserExternalStore _userExternalStore;
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
        IUserExternalStore userExternalStore)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                                    ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _userExternalStore = userExternalStore;
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
                    LastLoginDateUtc = _dateTimeService.UtcNow,
                }, cancellationToken);
        }
        else
        {
            user.LastLoginDateUtc = _dateTimeService.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<QueryablePaging<UserDto>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        await _context
            .Users
            .ProjectToType<UserDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

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

    public async Task<UserDto> UpdateUserStatusAsync(Guid id, UserStatus userStatus,
        CancellationToken cancellationToken = default)
    {
        // TODO: Move the same code into separated method
        var user = await _context
                       .Users
                       .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), id);

        switch (userStatus)
        {
            case UserStatus.Deactivated:
                user.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                user.ReactivationDateTimeUtc = null;
                user.UserStatus = UserStatus.Deactivated;
                break;
            case UserStatus.Active:
                user.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                user.DeactivationDateTimeUtc = null;
                user.UserStatus = UserStatus.Active;
                break;
        }

        user.UserStatus = userStatus;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) status was changed to {newUserStatus} by {@userId}",
            _dateTimeService.UtcNow,
            user.Id,
            userStatus,
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
        user.UserStatus = UserStatus.Active;

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

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) was created by {@userId}",
            _dateTimeService.UtcNow,
            user.Id,
            _currentUserService.UserId);

        return user.Adapt<UserDto>();
    }

    public async Task<byte[]> ExportUsersAsync(Guid tenantId, FileType fileType, string ianaTimeZone, CancellationToken cancellationToken)
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
            u.DeactivationDateTimeUtc = u.DeactivationDateTimeUtc.ConvertUtcDateToTimeZone(ianaTimeZone);
            u.ReactivationDateTimeUtc = u.ReactivationDateTimeUtc.ConvertUtcDateToTimeZone(ianaTimeZone);
            u.CreatedDateUtc = u.CreatedDateUtc.ConvertUtcDateToTimeZone(ianaTimeZone);
            u.LastLoginDateUtc = u.LastLoginDateUtc.ConvertUtcDateToTimeZone(ianaTimeZone);
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

    private async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(x => x.Email == email, cancellationToken);

}
