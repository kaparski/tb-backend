using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts;

public interface IAccountService
{
    IQueryable<AccountDto> QueryAccounts();
    
    Task<byte[]> ExportAccountsAsync(FileType fileType,
        CancellationToken cancellationToken = default);
}
