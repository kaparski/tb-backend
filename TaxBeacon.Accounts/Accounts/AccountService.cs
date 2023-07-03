using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.Accounts.Accounts;

public class AccountService: IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    public AccountService(ILogger<AccountService> logger,
        IAccountDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IEnumerable<IListToFileConverter> listToFileConverters)
    {
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
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
}
