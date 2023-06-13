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
        _context.AccountsView
            .AsNoTracking()
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .Select(av => new AccountDto
            {
                Id = av.Id,
                Name = av.Name,
                State = av.State,
                City = av.City,
                AccountType = av.AccountType,
                Client = _context.Clients
                    .Where(c => c.AccountId == av.Id)
                    .Select(c => new ClientDto(c.State, c.Status))
                    .SingleOrDefault(),
                // Referral = _context.Referrals
                //     .Where(r => r.AccountId == av.Id)
                //     .Select(r => new ReferralDto(r.State, r.Status))
                //     .SingleOrDefault(),
            });
}
