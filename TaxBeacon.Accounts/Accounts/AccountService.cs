using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Factories;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Accounts;

public class AccountService: IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IImmutableDictionary<(AccountEventType, uint), IAccountActivityFactory> _activityFactories;

    public AccountService(ILogger<AccountService> logger,
        IAccountDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IEnumerable<IAccountActivityFactory> activityFactories)
    {
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _activityFactories = activityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                             ?? ImmutableDictionary<(AccountEventType, uint), IAccountActivityFactory>.Empty;
    }

    public IQueryable<AccountDto> QueryAccounts() =>
        _context.AccountsView
            .Where(av => av.TenantId == _currentUserService.TenantId)
            .ProjectToType<AccountDto>();

    public async Task<OneOf<AccountDetailsDto, NotFound>> GetAccountDetailsByIdAsync(Guid id,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default)
    {
        var accountDetailsQuery = accountInfoType switch
        {
            AccountInfoType.Full => _context.Accounts
                .Include(a => a.Client)
                .ThenInclude(c => c!.ClientManagers)
                .Include(a => a.Client)
                .ThenInclude(c => c!.PrimaryContact)
                .Include(a => a.Referral),
            AccountInfoType.Client => _context.Accounts
                .Include(a => a.Client)
                .ThenInclude(c => c!.ClientManagers)
                .Include(a => a.Client)
                .ThenInclude(c => c!.PrimaryContact),
            AccountInfoType.Referral => _context.Accounts.Include(a => a.Referral),
            _ => _context.Accounts.AsQueryable()
        };

        var accountDetails = await accountDetailsQuery
            .Where(a => a.Id == id && a.TenantId == _currentUserService.TenantId)
            .ProjectToType<AccountDetailsDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return accountDetails is not null
            ? accountDetails
            : new NotFound();
    }

    public async Task<byte[]> ExportAccountsAsync(FileType fileType,
        CancellationToken cancellationToken = default)
    {
        var exportAccounts = await _context
            .AccountsView
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .OrderBy(a => a.Name)
            .ProjectToType<AccountExportDto>()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Accounts export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportAccounts);
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id,
        AccountInfoType accountInfoType,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var accountParts = accountInfoType switch
        {
            AccountInfoType.Full => Enum.GetValues<AccountPartActivityType>(),
            AccountInfoType.Client => new[] { AccountPartActivityType.General, AccountPartActivityType.Client },
            AccountInfoType.Referral => new[] { AccountPartActivityType.General, AccountPartActivityType.Referral },
            _ => new[] { AccountPartActivityType.General }
        };

        var query = _context.AccountActivityLogs
            .Where(log => log.AccountId == id
                          && log.TenantId == _currentUserService.TenantId
                          && accountParts.Contains(log.AccountPartType));

        var count = await query.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await query
            .OrderByDescending(log => log.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _activityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<AccountDetailsDto, NotFound>> UpdateClientStatusAsync(Guid accountId,
        Status status,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default)
    {
        var currentTenantId = _currentUserService.TenantId;

        var client = await _context
            .Clients
            .Include(c => c.ClientManagers)
            .Include(c => c.PrimaryContact)
            .Where(x => x.AccountId == accountId && x.TenantId == currentTenantId)
            .SingleOrDefaultAsync(cancellationToken);

        if (client == null)
        {
            return new NotFound();
        }

        switch (status)
        {
            case Status.Deactivated:
                client.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                client.ReactivationDateTimeUtc = null;
                break;
            case Status.Active:
                client.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                client.DeactivationDateTimeUtc = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        client.Status = status;

        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        var activityLog = status switch
        {
            Status.Active => new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = client.AccountId,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ClientReactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = AccountEventType.ClientReactivated,
                AccountPartType = AccountPartActivityType.Client
            },
            Status.Deactivated => new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = client.AccountId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ClientDeactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = AccountEventType.ClientDeactivated,
                AccountPartType = AccountPartActivityType.Client
            },
            _ => throw new InvalidOperationException()
        };

        await _context.AccountActivityLogs.AddAsync(activityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Client ({accountId}) status was changed to {newStatus} by {@userId}",
            _dateTimeService.UtcNow,
            client.AccountId,
            status,
            _currentUserService.UserId);

        return await GetAccountDetailsByIdAsync(accountId, accountInfoType, cancellationToken);
    }

    public IQueryable<ClientProspectDto> QueryClientsProspects() =>
        _context.Clients
            .Where(x => x.TenantId == _currentUserService.TenantId && x.State == ClientState.ClientProspect.Name)
            .Select(x => new ClientProspectDto
            {
                AccountId = x.AccountId,
                Name = x.Account.Name,
                City = x.Account.City,
                State = x.Account.State,
                Status = x.Status,
                DaysOpen = x.DaysOpen
            });
}
