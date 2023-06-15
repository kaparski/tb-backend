using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

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
        ;
    }

    public IQueryable<AccountDto> QueryAccounts() =>
        from av in _context.AccountsView
        join c in _context.Clients on av.Id equals c.AccountId into clients
        from accountClients in clients.DefaultIfEmpty()
        join r in _context.Referrals on av.Id equals r.AccountId into referrals
        from accountReferrals in referrals.DefaultIfEmpty()
        where av.TenantId == _currentUserService.TenantId
        select new AccountDto
        {
            Id = av.Id,
            Name = av.Name,
            AccountType = av.AccountType,
            City = av.City,
            State = av.State,
            Client = accountClients == null ? null : new ClientDto(accountClients.State, accountClients.Status),
            Referral = accountReferrals == null ? null : new ReferralDto(accountReferrals.State, accountReferrals.Status),
        };

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
