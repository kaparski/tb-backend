using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Factories;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.Administration.Users;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Extensions;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration;

namespace TaxBeacon.Accounts.Accounts;

public class AccountService: IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountDbContext _context;
    private readonly ITaxBeaconDbContext _administrationContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IUserService _userService;
    private readonly INaicsService _naicsService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    private readonly IImmutableDictionary<(AccountEventType, uint), IActivityFactory<AccountEventType>>
        _activityFactories;

    public AccountService(ILogger<AccountService> logger,
        IAccountDbContext context,
        ITaxBeaconDbContext administrationContext,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IEnumerable<IActivityFactory<AccountEventType>> activityFactories,
        IUserService userService,
        INaicsService naicsService)
    {
        _logger = logger;
        _context = context;
        _administrationContext = administrationContext;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _naicsService = naicsService;
        _userService = userService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _activityFactories = activityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                             ?? ImmutableDictionary<(AccountEventType, uint), IActivityFactory<AccountEventType>>.Empty;
    }

    public IQueryable<AccountDto> QueryAccounts() => _context.AccountsView
        .Where(av => av.TenantId == _currentUserService.TenantId)
        .GroupJoin(_context.ClientManagers, a => a.Id, m => m.AccountId,
            (a, m) => new AccountDto
            {
                AccountIdField = a.AccountId,
                State = a.State,
                ReferralStatus = a.ReferralStatus,
                ClientStatus = a.ClientStatus,
                AccountType = a.AccountType,
                Id = a.Id,
                City = a.City,
                ClientAccountManagers = a.ClientAccountManagers,
                ReferralAccountManagers = a.ReferralAccountManagers,
                ClientAccountManagerIds = m.Select(x => x.UserId),
                ReferralAccountManagerIds = _context.ReferralManagers
                    .Where(rm => rm.AccountId == a.Id)
                    .Select(rm => rm.UserId),
                ClientState = a.ClientState,
                Name = a.Name,
                ReferralType = a.ReferralType,
                OrganizationType = a.OrganizationType,
                ReferralState = a.ReferralState,
                NaicsCode = a.NaicsCode,
                NaicsCodeIndustry = a.NaicsCodeIndustry,
                Country = a.Country,
                County = a.County,
                Salespersons = a.Salespersons,
                EmployeeCount = a.EmployeeCount,
                AnnualRevenue = a.AnnualRevenue,
                ClientPrimaryContactId = a.ClientPrimaryContactId,
                ReferralPrimaryContactId = a.ReferralPrimaryContactId,
                SalespersonIds = _context.Salespersons.Where(s => s.AccountId == a.Id).Select(s => s.UserId),
                ContactIds = _context.AccountContacts.Where(s => s.AccountId == a.Id).Select(s => s.ContactId)
            });

    public async Task<OneOf<AccountDetailsDto, NotFound>> GetAccountDetailsByIdAsync(Guid id,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Accounts
            .Include(a => a.NaicsCode)
            .Include(a => a.Phones);

        var accountDetailsQuery = accountInfoType switch
        {
            AccountInfoType.Full => baseQuery
                .Include(a => a.Client)
                .ThenInclude(c => c!.ClientManagers)
                .Include(a => a.Client)
                .ThenInclude(c => c!.PrimaryContact)
                .Include(a => a.Referral),
            AccountInfoType.Client => baseQuery
                .Include(a => a.Client)
                .ThenInclude(c => c!.ClientManagers)
                .Include(a => a.Client)
                .ThenInclude(c => c!.PrimaryContact),
            AccountInfoType.Referral => baseQuery
                .Include(a => a.Referral),
            _ => _context.Accounts.AsQueryable()
        };

        var accountDetails = await accountDetailsQuery
            .Where(a => a.Id == id && a.TenantId == _currentUserService.TenantId)
            .ProjectToType<AccountDetailsDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return accountDetails is null ? new NotFound() : accountDetails;
    }

    public async Task<OneOf<AccountDetailsDto, InvalidOperation>> CreateAccountAsync(CreateAccountDto createAccountDto,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateAccountAsync(
            createAccountDto.AccountId,
            createAccountDto.Name,
            createAccountDto.Website,
            createAccountDto.SalespersonIds,
            createAccountDto.Phones,
            createAccountDto.PrimaryNaicsCode,
            cancellationToken);

        if (!validationResult.TryPickT0(out var ok, out var error))
        {
            return error;
        }

        var newAccount = createAccountDto.Adapt<Account>();
        newAccount.Id = Guid.NewGuid();
        newAccount.TenantId = _currentUserService.TenantId;
        await _context.Accounts.AddAsync(newAccount, cancellationToken);

        var (userFullName, userRoles) = _currentUserService.UserInfo;

        await _context.AccountPhones.AddRangeAsync(createAccountDto.Phones
            .Select(p =>
            {
                var phone = p.Adapt<AccountPhone>();
                phone.AccountId = newAccount.Id;
                phone.TenantId = newAccount.TenantId;
                return phone;
            }), cancellationToken);

        if (createAccountDto.Client is not null)
        {
            await AddClientAccountPartAsync(createAccountDto.Client,
                newAccount.Id,
                userFullName,
                userRoles,
                cancellationToken);
        }

        if (createAccountDto.Referral is not null)
        {
            await AddReferralAccountPart(createAccountDto.Referral,
                newAccount.Id,
                userFullName,
                userRoles,
                cancellationToken);
        }

        if (createAccountDto.SalespersonIds.Any())
        {
            await _context.Salespersons.AddRangeAsync(createAccountDto.SalespersonIds
                .Select(salespersonId => new AccountSalesperson
                {
                    TenantId = _currentUserService.TenantId, UserId = salespersonId, AccountId = newAccount.Id
                }), cancellationToken);

            var salespersons = await _administrationContext.Users
                .Where(u => createAccountDto.SalespersonIds.Contains(u.Id))
                .Select(u => u.FullName)
                .ToListAsync(cancellationToken);

            var assignSalespersonDateTime = _dateTimeService.UtcNow;

            await _context.AccountActivityLogs.AddAsync(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = newAccount.Id,
                Date = assignSalespersonDateTime,
                Revision = 1,
                EventType = AccountEventType.SalespersonAssigned,
                AccountPartType = AccountPartActivityType.General,
                Event = JsonSerializer.Serialize(new SalespersonAssignedEvent(
                    _currentUserService.UserId,
                    assignSalespersonDateTime,
                    userFullName,
                    userRoles,
                    string.Join(", ", salespersons)))
            }, cancellationToken);
        }


        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Account ({accountId} was created by {userId}",
            _dateTimeService.UtcNow,
            newAccount.Id,
            _currentUserService.UserId);

        return GetAccountDetailsByIdAsync(newAccount.Id, accountInfoType, cancellationToken).Result.AsT0;
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

        var accountsPhones = await _context.AccountPhones
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .ToListAsync(cancellationToken);

        exportAccounts.ForEach(e =>
        {
            e.Phones = string.Join("\n", accountsPhones.Where(p => p.AccountId == e.Id).Select(p =>
            {
                var number = e.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            e.Zip = e.Zip.ApplyZipMask();
            e.AnnualRevenueView = string.Format(new CultureInfo("en-US", false), "{0:c0}", e.AnnualRevenue);
        });

        _logger.LogInformation("{dateTime} - Accounts export was executed by {userId}",
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

    public async Task<OneOf<AccountDetailsDto, NotFound>> UpdateClientDetailsAsync(Guid accountId,
        UpdateClientDto updatedClient,
        CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .Include(c => c.ClientManagers)
            .SingleOrDefaultAsync(t => t.AccountId == accountId && t.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (client is null)
        {
            return new NotFound();
        }

        var (userFullName, userRoles) = _currentUserService.UserInfo;
        var previousValues = JsonSerializer.Serialize(client.Adapt<UpdateClientDto>());

        var eventDateTime = _dateTimeService.UtcNow;

        var currentManagersIds = client.ClientManagers.Select(cm => cm.UserId).ToList();

        // Removes association with ClientManagers
        var managersToRemove = _context.ClientManagers
            .Where(cm => currentManagersIds.Except(updatedClient.ClientManagersIds).Contains(cm.UserId));

        if (managersToRemove.Any())
        {
            _context.ClientManagers.RemoveRange(managersToRemove);

            var managersToRemoveIds = managersToRemove.Select(sp => sp.UserId).ToList();

            var accountManagersNames = await _administrationContext.Users
                .Where(u => managersToRemoveIds.Contains(u.Id))
                .Select(u => u.FullName)
                .ToListAsync(cancellationToken);

            var unassignManagerDateTime = _dateTimeService.UtcNow;

            await _context.AccountActivityLogs.AddAsync(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = client.AccountId,
                Date = unassignManagerDateTime,
                Revision = 1,
                EventType = AccountEventType.ClientAccountManagerUnassigned,
                AccountPartType = AccountPartActivityType.Client,
                Event = JsonSerializer.Serialize(new ClientAccountManagerUnassignedEvent(
                    _currentUserService.UserId,
                    unassignManagerDateTime,
                    userFullName,
                    userRoles,
                    string.Join(", ", accountManagersNames)))
            }, cancellationToken);
        }

        // Set up association with freshly added client managers
        var managersToAddIds = updatedClient.ClientManagersIds.Except(currentManagersIds);

        if (managersToAddIds.Any())
        {
            await _context.ClientManagers
                .AddRangeAsync(managersToAddIds
                    .Select(mta => new ClientManager
                    {
                        AccountId = accountId, TenantId = _currentUserService.TenantId, UserId = mta
                    }), cancellationToken);

            var accountManagersNames = await _administrationContext.Users
                .Where(u => managersToAddIds.Contains(u.Id))
                .Select(u => u.FullName)
                .ToListAsync(cancellationToken);

            var assignManagersDateTime = _dateTimeService.UtcNow;

            await _context.AccountActivityLogs.AddAsync(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = client.AccountId,
                Date = assignManagersDateTime,
                Revision = 1,
                EventType = AccountEventType.ClientAccountManagerAssigned,
                AccountPartType = AccountPartActivityType.Client,
                Event = JsonSerializer.Serialize(new ClientAccountManagerAssignedEvent(
                    _currentUserService.UserId,
                    assignManagersDateTime,
                    userFullName,
                    userRoles,
                    string.Join(", ", accountManagersNames)))
            }, cancellationToken);
        }

        if (managersToRemove.Any() || managersToAddIds.Any())
        {
            client.LastModifiedDateTimeUtc = eventDateTime;
        }

        var adaptedCurrentClient = client.Adapt<UpdateClientDto>();
        if (!updatedClient.Equals(adaptedCurrentClient))
        {
            updatedClient.Adapt(client);

            await _context.AccountActivityLogs.AddAsync(new AccountActivityLog
            {
                TenantId = client.TenantId,
                AccountId = client.AccountId,
                Date = eventDateTime,
                Revision = 1,
                EventType = AccountEventType.ClientDetailsUpdated,
                AccountPartType = AccountPartActivityType.Client,
                Event = JsonSerializer.Serialize(new ClientUpdatedEvent(
                    _currentUserService.UserId,
                    userRoles ?? string.Empty,
                    userFullName,
                    eventDateTime,
                    previousValues,
                    JsonSerializer.Serialize(updatedClient)))
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Client account ({serviceArea}) was updated by {userId}",
            eventDateTime,
            accountId,
            _currentUserService.UserId);

        return await GetAccountDetailsByIdAsync(accountId, AccountInfoType.Client, cancellationToken);
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

        if (client.State == ClientState.ClientProspect.Name && status == Status.Active)
        {
            client.ActivationDateTimeUtc = _dateTimeService.UtcNow;
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

        _logger.LogInformation("{dateTime} - Client ({accountId}) status was changed to {newStatus} by {userId}",
            _dateTimeService.UtcNow,
            client.AccountId,
            status,
            _currentUserService.UserId);

        return await GetAccountDetailsByIdAsync(accountId, accountInfoType, cancellationToken);
    }

    public IQueryable<ClientProspectDto> QueryClientsProspects() => _context.ClientsView
        .Where(x => x.TenantId == _currentUserService.TenantId && x.ClientState == ClientState.ClientProspect.Name)
        .GroupJoin(_context.Salespersons, s => s.AccountId, c => c.AccountId,
            (x, s) => new ClientProspectDto
            {
                AccountId = x.AccountId,
                AccountIdField = x.AccountIdField,
                Name = x.Name,
                City = x.City,
                State = x.State,
                Status = x.Status,
                DaysOpen = x.DaysOpen,
                Salespersons = x.Salespersons,
                SalespersonIds = s.Select(p => p.UserId),
                CreatedDateTimeUtc = x.CreatedDateTimeUtc,
                ReactivationDateTimeUtc = x.ReactivationDateTimeUtc,
                DeactivationDateTimeUtc = x.DeactivationDateTimeUtc,
                NaicsCode = x.NaicsCode,
                NaicsCodeIndustry = x.NaicsCodeIndustry,
                Country = x.Country,
                County = x.County,
                EmployeeCount = x.EmployeeCount,
                AnnualRevenue = x.AnnualRevenue,
                PrimaryContactId = x.PrimaryContactId,
                AccountManagerIds = _context.ClientManagers.Where(m => m.AccountId == x.AccountId)
                    .Select(m => m.UserId)
            });

    public async Task<byte[]> ExportClientsProspectsAsync(FileType fileType, CancellationToken cancellationToken)
    {
        var clientsProspects = await _context
            .AccountsView
            .Where(x => x.TenantId == _currentUserService.TenantId && x.ClientState == ClientState.ClientProspect.Name)
            .OrderBy(x => x.Name)
            .ProjectToType<ClientProspectExportDto>()
            .ToListAsync(cancellationToken);

        var accountsPhones = await _context.AccountPhones
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .ToListAsync(cancellationToken);

        clientsProspects.ForEach(cp =>
        {
            cp.Phones = string.Join("\n", accountsPhones.Where(p => p.AccountId == cp.Id).Select(p =>
            {
                var number = cp.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            cp.Zip = cp.Zip.ApplyZipMask();
            cp.AnnualRevenueView = string.Format(new CultureInfo("en-US", false), "{0:c0}", cp.AnnualRevenue);
        });

        _logger.LogInformation("{dateTime} - Client prospects export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(clientsProspects);
    }

    public IQueryable<ClientDto> QueryClients() => _context.ClientsView
        .Where(x => x.TenantId == _currentUserService.TenantId && x.ClientState == ClientState.Client.Name)
        .GroupJoin(_context.ClientManagers, c => c.AccountId, cm => cm.AccountId,
            (c, cms) => new ClientDto
            {
                Id = c.AccountId,
                AccountIdField = c.AccountIdField,
                Name = c.Name,
                City = c.City,
                State = c.State,
                PrimaryContactName = c.PrimaryContactName ?? string.Empty,
                Status = c.Status,
                AccountManagers = c.AccountManagers,
                AccountManagerIds = cms.Select(p => p.UserId),
                CreatedDateTimeUtc = c.CreatedDateTimeUtc,
                DeactivationDateTimeUtc = c.DeactivationDateTimeUtc,
                ReactivationDateTimeUtc = c.ReactivationDateTimeUtc,
                NaicsCode = c.NaicsCode,
                NaicsCodeIndustry = c.NaicsCodeIndustry,
                Country = c.Country,
                County = c.County,
                EmployeeCount = c.EmployeeCount,
                AnnualRevenue = c.AnnualRevenue,
                PrimaryContactId = c.PrimaryContactId,
                Salespersons = c.Salespersons,
                SalespersonIds = _context.Salespersons.Where(s => s.AccountId == c.AccountId).Select(s => s.UserId)
            });

    public async Task<byte[]> ExportClientsAsync(FileType fileType, CancellationToken cancellationToken)
    {
        var clients = await _context
            .AccountsView
            .Where(x => x.TenantId == _currentUserService.TenantId && x.ClientState == ClientState.Client.Name)
            .OrderBy(x => x.Name)
            .ProjectToType<ClientExportDto>()
            .ToListAsync(cancellationToken);

        var accountsPhones = await _context.AccountPhones
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .ToListAsync(cancellationToken);

        clients.ForEach(c =>
        {
            c.Phones = string.Join("\n", accountsPhones.Where(p => p.AccountId == c.Id).Select(p =>
            {
                var number = c.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            c.Zip = c.Zip.ApplyZipMask();
            c.AnnualRevenueView = string.Format(new CultureInfo("en-US", false), "{0:c0}", c.AnnualRevenue);
        });

        _logger.LogInformation("{dateTime} - Clients export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(clients);
    }

    public async Task<
        OneOf<
            AccountDetailsDto,
            NotFound,
            InvalidOperation
        >
    > UpdateAccountProfileAsync(Guid id, UpdateAccountProfileDto updateAccountProfileDto,
        CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .Include(a => a.Phones)
            .Include(a => a.Salespersons)
            .SingleOrDefaultAsync(
                a => a.Id == id && a.TenantId == _currentUserService.TenantId, cancellationToken);

        if (account is null)
        {
            return new NotFound();
        }

        var validationResult = await ValidateAccountAsync(
            updateAccountProfileDto.AccountId,
            updateAccountProfileDto.Name,
            updateAccountProfileDto.Website,
            updateAccountProfileDto.SalespersonIds,
            updateAccountProfileDto.Phones,
            updateAccountProfileDto.PrimaryNaicsCode,
            cancellationToken,
            account.Id);

        if (!validationResult.TryPickT0(out var ok, out var error))
        {
            return error;
        }

        var currentAccount = account.Adapt<UpdateAccountProfileDto>();

        var (userFullName, userRoles) = _currentUserService.UserInfo;
        var previousValues = JsonSerializer.Serialize(account.Adapt<UpdateAccountProfileDto>());

        var eventDateTime = _dateTimeService.UtcNow;

        var currentSalespersonIds = account.Salespersons.Select(sp => sp.UserId).ToArray();

        // Unassign salespersons
        var salespersonsToRemove = account.Salespersons
            .Where(sp =>
                currentSalespersonIds.Except(updateAccountProfileDto.SalespersonIds).Contains(sp.UserId));

        if (salespersonsToRemove.Any())
        {
            _context.Salespersons.RemoveRange(salespersonsToRemove);

            var salespersonToRemoveIds = salespersonsToRemove.Select(sp => sp.UserId).ToList();
            var salespersonNames = await _administrationContext.Users
                .Where(u => salespersonToRemoveIds.Contains(u.Id))
                .Select(u => u.FullName)
                .ToListAsync(cancellationToken);

            var unassignSalespersonDateTime = _dateTimeService.UtcNow;

            await _context.AccountActivityLogs.AddAsync(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = account.Id,
                Date = unassignSalespersonDateTime,
                Revision = 1,
                EventType = AccountEventType.SalespersonUnassigned,
                AccountPartType = AccountPartActivityType.General,
                Event = JsonSerializer.Serialize(new SalespersonUnassignedEvent(
                    _currentUserService.UserId,
                    unassignSalespersonDateTime,
                    userFullName,
                    userRoles,
                    string.Join(", ", salespersonNames)))
            }, cancellationToken);
        }

        //Assign salespersons

        var salespersonsToAdd = updateAccountProfileDto.SalespersonIds
            .Except(currentSalespersonIds)
            .Select(salepersonId => new AccountSalesperson
            {
                TenantId = _currentUserService.TenantId, UserId = salepersonId, AccountId = account.Id
            });

        if (salespersonsToAdd.Any())
        {
            await _context.Salespersons.AddRangeAsync(salespersonsToAdd, cancellationToken);

            var salespersonToAddIds = salespersonsToAdd.Select(sp => sp.UserId).ToList();
            var salespersonNames = await _administrationContext.Users
                .Where(u => salespersonToAddIds.Contains(u.Id))
                .Select(u => u.FullName)
                .ToListAsync(cancellationToken);

            var assignSalespersonDateTime = _dateTimeService.UtcNow;

            await _context.AccountActivityLogs.AddAsync(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = account.Id,
                Date = assignSalespersonDateTime,
                Revision = 1,
                EventType = AccountEventType.SalespersonAssigned,
                AccountPartType = AccountPartActivityType.General,
                Event = JsonSerializer.Serialize(new SalespersonAssignedEvent(
                    _currentUserService.UserId,
                    assignSalespersonDateTime,
                    userFullName,
                    userRoles,
                    string.Join(", ", salespersonNames)))
            }, cancellationToken);
        }

        if (salespersonsToAdd.Any() || salespersonsToRemove.Any())
        {
            account.LastModifiedDateTimeUtc = eventDateTime;
        }

        var currentPhoneIds = account.Phones.Select(p => p.Id).ToArray();
        var newPhoneIds = updateAccountProfileDto.Phones.Select(p => p.Id);
        var phonesToRemove = account.Phones
            .Where(p =>
                currentPhoneIds.Except(newPhoneIds).Contains(p.Id))
            .ToList();
        var phonesToAdd = updateAccountProfileDto.Phones
            .Where(p =>
                newPhoneIds.Except(currentPhoneIds).Contains(p.Id))
            .Select(p =>
            {
                var phone = p.Adapt<AccountPhone>();
                phone.AccountId = account.Id;
                phone.TenantId = _currentUserService.TenantId;
                return phone;
            })
            .ToList();
        var phoneDtosToUpdate = updateAccountProfileDto.Phones
            .Where(p => newPhoneIds.Intersect(currentPhoneIds).Contains(p.Id))
            .ToDictionary(p => p.Id);

        foreach (var phone in account.Phones.Where(p => phoneDtosToUpdate.ContainsKey(p.Id)))
        {
            _context.AccountPhones.Update(phoneDtosToUpdate[phone.Id].Adapt(phone));
        }

        _context.AccountPhones.RemoveRange(phonesToRemove);
        await _context.AccountPhones.AddRangeAsync(phonesToAdd, cancellationToken);

        if (!currentAccount.Equals(updateAccountProfileDto))
        {
            updateAccountProfileDto.Adapt(account);

            await _context.AccountActivityLogs.AddAsync(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = account.Id,
                Date = eventDateTime,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new AccountProfileUpdatedEvent(_currentUserService.UserId,
                        userFullName,
                        userRoles,
                        eventDateTime,
                        previousValues,
                        JsonSerializer.Serialize(updateAccountProfileDto))),
                EventType = AccountEventType.AccountProfileUpdated,
                AccountPartType = AccountPartActivityType.General
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Account ({account}) profile details were updated by {userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        var accountProfileDetailsDto =
            await GetAccountDetailsByIdAsync(account.Id, AccountInfoType.General, cancellationToken);

        return accountProfileDetailsDto.AsT0;
    }

    public IQueryable<ReferralPartnerDto> QueryReferralPartners() => _context.ReferralsView
        .Where(x => x.TenantId == _currentUserService.TenantId && x.ReferralState == ReferralState.ReferralPartner.Name)
        .GroupJoin(_context.Salespersons, s => s.AccountId, c => c.AccountId,
            (x, s) => new ReferralPartnerDto
            {
                Id = x.AccountId,
                AccountIdField = x.AccountIdField,
                Name = x.Name,
                City = x.City,
                State = x.State,
                Status = x.Status,
                AccountManagers = x.AccountManagers,
                AccountManagerIds =
                    _context.ReferralManagers.Where(s => s.AccountId == x.AccountId).Select(s => s.UserId),
                SalespersonIds = s.Select(p => p.UserId),
                CreatedDateTimeUtc = x.CreatedDateTimeUtc,
                ReactivationDateTimeUtc = x.ReactivationDateTimeUtc,
                DeactivationDateTimeUtc = x.DeactivationDateTimeUtc,
                NaicsCode = x.NaicsCode,
                NaicsCodeIndustry = x.NaicsCodeIndustry,
                Country = x.Country,
                County = x.County,
                Type = x.Type,
                OrganizationType = x.OrganizationType,
                PrimaryContactName = x.PrimaryContactName ?? string.Empty,
                PrimaryContactId = x.PrimaryContactId
            });

    public async Task<byte[]> ExportReferralPartnersAsync(FileType fileType, CancellationToken cancellationToken)
    {
        var referralPartners = await _context
            .AccountsView
            .Where(x => x.TenantId == _currentUserService.TenantId &&
                        x.ReferralState == ReferralState.ReferralPartner.Name)
            .OrderBy(x => x.Name)
            .ProjectToType<ReferralPartnerExportDto>()
            .ToListAsync(cancellationToken);

        var accountsPhones = await _context.AccountPhones
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .ToListAsync(cancellationToken);

        referralPartners.ForEach(cp =>
        {
            cp.Phones = string.Join("\n", accountsPhones.Where(p => p.AccountId == cp.Id).Select(p =>
            {
                var number = cp.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            cp.Zip = cp.Zip.ApplyZipMask();
        });

        _logger.LogInformation("{dateTime} - Referral partners export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(referralPartners);
    }

    public IQueryable<ReferralProspectDto> QueryReferralsProspects() => _context.ReferralsView
        .Where(x => x.TenantId == _currentUserService.TenantId &&
                    x.ReferralState == ReferralState.ReferralProspect.Name)
        .GroupJoin(_context.Salespersons, s => s.AccountId, c => c.AccountId,
            (x, s) => new ReferralProspectDto
            {
                Id = x.AccountId,
                AccountIdField = x.AccountIdField,
                Name = x.Name,
                City = x.City,
                State = x.State,
                Status = x.Status,
                DaysOpen = x.DaysOpen,
                AccountManagerIds =
                    _context.ReferralManagers.Where(s => s.AccountId == x.AccountId).Select(s => s.UserId),
                Salespersons = x.Salespersons,
                SalespersonIds = s.Select(p => p.UserId),
                CreatedDateTimeUtc = x.CreatedDateTimeUtc,
                ReactivationDateTimeUtc = x.ReactivationDateTimeUtc,
                DeactivationDateTimeUtc = x.DeactivationDateTimeUtc,
                NaicsCode = x.NaicsCode,
                NaicsCodeIndustry = x.NaicsCodeIndustry,
                Country = x.Country,
                County = x.County,
                Type = x.Type,
                OrganizationType = x.OrganizationType,
                PrimaryContactId = x.PrimaryContactId
            });

    public async Task<byte[]> ExportReferralProspectsAsync(FileType fileType, CancellationToken cancellationToken)
    {
        var referralProspects = await _context
            .AccountsView
            .Where(x => x.TenantId == _currentUserService.TenantId &&
                        x.ReferralState == ReferralState.ReferralProspect.Name)
            .OrderBy(x => x.Name)
            .ProjectToType<ReferralProspectExportDto>()
            .ToListAsync(cancellationToken);

        var accountsPhones = await _context.AccountPhones
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .ToListAsync(cancellationToken);

        referralProspects.ForEach(cp =>
        {
            cp.Phones = string.Join("\n", accountsPhones.Where(p => p.AccountId == cp.Id).Select(p =>
            {
                var number = cp.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            cp.Zip = cp.Zip.ApplyZipMask();
        });

        _logger.LogInformation("{dateTime} - Referral prospects export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(referralProspects);
    }

    private async Task<OneOf<Success, InvalidOperation>> ValidateAccountAsync(
           string accountId,
           string name,
           string website,
           IEnumerable<Guid> salespersonIds,
           IEnumerable<CreateUpdatePhoneDto> phones,
           int? primaryNaics,
           CancellationToken cancellationToken,
           Guid? id = null)
    {
        if (await _context.Accounts.AnyAsync(
                    a => a.AccountId == accountId && a.TenantId == _currentUserService.TenantId && (a.Id != id || id == null), cancellationToken))
        {
            return new InvalidOperation(
                $"account with the same account ID already exists",
                nameof(accountId));
        }

        if (await _context.Accounts.AnyAsync(
                a => a.Name == name && a.Id != id && a.TenantId == _currentUserService.TenantId, cancellationToken))
        {
            return new InvalidOperation(
                $"account with the same name already exists",
                nameof(name));
        }

        if (await _context.Accounts.AnyAsync(
                a => a.Website == website && a.Id != id && a.TenantId == _currentUserService.TenantId, cancellationToken))
        {
            return new InvalidOperation(
                $"account with the same website already exists",
                nameof(website));
        }

        foreach (var salespersonId in salespersonIds)
        {
            var exists = await _userService
                .UserExistsInTenantAsync(salespersonId, _currentUserService.TenantId, cancellationToken);

            if (!exists)
            {
                return new InvalidOperation(
                    $"User {salespersonId} does not exist in tenant {_currentUserService.TenantId}",
                    nameof(salespersonIds));
            }
        }

        foreach (var phone in phones)
        {
            // Check if a phone being updated belongs to specified account or is a newly created phone,
            // and proceed if it's valid
            var invalid = await _context.AccountPhones
                .AnyAsync(p => p.Id == phone.Id && p.AccountId != id, cancellationToken);

            if (invalid)
            {
                return new InvalidOperation(
                    $"Phone {phone.Id} belongs to another account, entity, location or contact",
                    nameof(phones));
            }
        }

        if (primaryNaics != null)
        {
            if (!await _naicsService.IsNaicsValidAsync(primaryNaics, cancellationToken))
            {
                return new InvalidOperation(
                    $"This {primaryNaics} NAICS code doesn't exist",
                    nameof(primaryNaics));
            }
        }

        return new Success();
    }

    public async Task<OneOf<string, InvalidOperation>> GenerateUniqueAccountIdAsync(CancellationToken cancellationToken)
    {
        var r = new Random();
        var code = "A" + r.Next(1_000_000, 9_999_999).ToString();
        var safeExit = 0;
        var tenantId = _currentUserService.TenantId;
        while (safeExit < 10 &&
               await _context.Accounts.AnyAsync(x => x.AccountId == code && x.TenantId == tenantId, cancellationToken))
        {
            code = "A" + r.Next(1_000_000, 9_999_999).ToString();
            safeExit++;
        }

        if (safeExit >= 10)
        {
            _logger.LogError("Failed to generate account Id. Attempts count exceeded the limit.");
            return new InvalidOperation("Failed to generate the code");
        }

        return code;
    }

    private async Task AddClientAccountPartAsync(CreateClientDto createClientDto,
        Guid newAccountId,
        string currentUserFullName,
        string currentUserRoles,
        CancellationToken cancellationToken)
    {
        var newClient = createClientDto.Adapt<Client>();
        newClient.AccountId = newAccountId;
        newClient.TenantId = _currentUserService.TenantId;
        newClient.State = ClientState.ClientProspect.Name;
        newClient.Status = Status.Active;

        _context.Clients.Add(newClient);
        _context.AccountActivityLogs.Add(
            new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = newAccountId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                EventType = AccountEventType.ClientAccountCreated,
                AccountPartType = AccountPartActivityType.Client,
                Event = JsonSerializer.Serialize(new ClientAccountCreatedEvent(
                    _currentUserService.UserId,
                    _dateTimeService.UtcNow,
                    currentUserFullName,
                    currentUserRoles))
            });

        if (createClientDto.ClientManagersIds.Any())
        {
            _context.ClientManagers
                .AddRange(createClientDto.ClientManagersIds
                    .Select(id => new ClientManager
                    {
                        AccountId = newAccountId, TenantId = _currentUserService.TenantId, UserId = id
                    }));

            var clientManagersNames = await _administrationContext.Users
                .Where(u => createClientDto.ClientManagersIds.Contains(u.Id))
                .Select(u => u.FullName)
                .ToListAsync(cancellationToken);

            _context.AccountActivityLogs.Add(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = newAccountId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                EventType = AccountEventType.ClientAccountManagerAssigned,
                AccountPartType = AccountPartActivityType.Client,
                Event = JsonSerializer.Serialize(new ClientAccountManagerAssignedEvent(
                    _currentUserService.UserId,
                    _dateTimeService.UtcNow,
                    currentUserFullName,
                    currentUserRoles,
                    string.Join(", ", clientManagersNames)))
            });
        }
    }

    private async Task AddReferralAccountPart(CreateReferralDto createReferralDto,
        Guid newAccountId,
        string currentUserFullName,
        string currentUserRoles,
        CancellationToken cancellationToken)
    {
        var newReferral = createReferralDto.Adapt<Referral>();
        newReferral.AccountId = newAccountId;
        newReferral.TenantId = _currentUserService.TenantId;
        newReferral.State = ReferralState.ReferralProspect.Name;
        newReferral.Status = Status.Active;

        _context.Referrals.Add(newReferral);
        _context.AccountActivityLogs.Add(
            new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = newAccountId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                EventType = AccountEventType.ReferralAccountCreated,
                AccountPartType = AccountPartActivityType.Referral,
                Event = JsonSerializer.Serialize(new ReferralAccountCreatedEvent(
                    _currentUserService.UserId,
                    currentUserFullName,
                    currentUserRoles,
                    _dateTimeService.UtcNow))
            });

        if (createReferralDto.ReferralManagersIds.Any())
        {
            _context.ReferralManagers
                .AddRange(createReferralDto.ReferralManagersIds
                    .Select(id => new ReferralManager
                    {
                        AccountId = newAccountId, TenantId = _currentUserService.TenantId, UserId = id
                    }));

            var clientManagersNames = await _administrationContext.Users
                .Where(u => createReferralDto.ReferralManagersIds.Contains(u.Id))
                .Select(u => u.FullName)
                .ToListAsync(cancellationToken);

            _context.AccountActivityLogs.Add(new AccountActivityLog
            {
                TenantId = _currentUserService.TenantId,
                AccountId = newAccountId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                EventType = AccountEventType.ReferralAccountManagerAssigned,
                AccountPartType = AccountPartActivityType.Referral,
                Event = JsonSerializer.Serialize(new ReferralAccountManagerAssignedEvent(
                    _currentUserService.UserId,
                    currentUserFullName,
                    currentUserRoles,
                    _dateTimeService.UtcNow,
                    string.Join(", ", clientManagersNames)))
            });
        }
    }
}
