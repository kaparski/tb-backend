using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.Accounts;

public class AccountService: IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AccountService(ILogger<AccountService> logger,
        IAccountDbContext context,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
    }

    public IQueryable<AccountDto> GetAccounts() =>
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
}
