using TaxBeacon.Accounts.Accounts.Models;

namespace TaxBeacon.Accounts.Accounts;

public interface IAccountService
{
    IQueryable<AccountDto> GetAccounts();
}
