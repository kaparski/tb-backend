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
using TaxBeacon.Common.Extensions;
using System.Collections.Immutable;
using TimeZoneNames;

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

    public async Task<UserDto> UpdateUserStatusAsync(Guid id, Status status,
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
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) was created by {@userId}",
            _dateTimeService.UtcNow,
            user.Id,
            _currentUserService.UserId);

        return user.Adapt<UserDto>();
    }

    public async Task<byte[]> ExportUsersAsync(Guid tenantId, FileType fileType, string ianaTimeZone,
        CancellationToken cancellationToken)
    {
        tenantId = tenantId != default ? tenantId : (await _context.Tenants.FirstAsync(cancellationToken)).Id;

        var exportUsers = await _context.Users
            .Where(u => u.TenantUsers.Any(tu => tu.TenantId == tenantId))
            .OrderBy(u => u.Email)
            .ProjectToType<UserExportModel>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        TimeZoneInfo.TryConvertIanaIdToWindowsId(ianaTimeZone, out var windowsId);

        windowsId ??= "UTC";

        var tz = TimeZoneInfo.FindSystemTimeZoneById(windowsId);
        var abbreviations = TZNames.GetAbbreviationsForTimeZone(windowsId, "en-US");

        const string format = $"MM.dd.yyyy hh:mm:ss tt";

        exportUsers.ForEach(u =>
        {
            u.DeactivationDateTimeUtc = u.DeactivationDateTimeUtc.ConvertUtcDateToTimeZone(ianaTimeZone);
            u.ReactivationDateTimeUtc = u.ReactivationDateTimeUtc.ConvertUtcDateToTimeZone(ianaTimeZone);
            u.CreatedDateTimeUtc = u.CreatedDateTimeUtc.ConvertUtcDateToTimeZone(ianaTimeZone);
            u.LastLoginDateTimeUtc = u.LastLoginDateTimeUtc.ConvertUtcDateToTimeZone(ianaTimeZone);

            if (u.DeactivationDateTimeUtc.HasValue)
            {
                u.DeactivationDateTimeUtcView = tz.IsDaylightSavingTime(u.DeactivationDateTimeUtc.Value)
                    ? $"{u.DeactivationDateTimeUtc.Value.ToString(format)} {abbreviations.Daylight}"
                    : $"{u.DeactivationDateTimeUtc.Value.ToString(format)} {abbreviations.Standard}";
            }
            if (u.ReactivationDateTimeUtc.HasValue)
            {
                u.ReactivationDateTimeUtcView = tz.IsDaylightSavingTime(u.ReactivationDateTimeUtc.Value)
                    ? $"{u.ReactivationDateTimeUtc.Value.ToString(format)} {abbreviations.Daylight}"
                    : $"{u.ReactivationDateTimeUtc.Value.ToString(format)} {abbreviations.Standard}";
            }
            if (u.LastLoginDateTimeUtc.HasValue)
            {
                u.LastLoginDateTimeUtcView = tz.IsDaylightSavingTime(u.LastLoginDateTimeUtc.Value)
                    ? $"{u.LastLoginDateTimeUtc.Value.ToString(format)} {abbreviations.Daylight}"
                    : $"{u.LastLoginDateTimeUtc.Value.ToString(format)} {abbreviations.Standard}";
            }

            u.CreatedDateTimeUtcView = tz.IsDaylightSavingTime(u.CreatedDateTimeUtc)
                ? $"{u.CreatedDateTimeUtc.ToString(format)} {abbreviations.Daylight}"
                : $"{u.CreatedDateTimeUtc.ToString(format)} {abbreviations.Standard}";
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
