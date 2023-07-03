using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts;

public interface IAccountService
{
    IQueryable<AccountDto> QueryAccounts();

    Task<OneOf<AccountDetailsDto, NotFound>> GetAccountDetailsByIdAsync(Guid id,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportAccountsAsync(FileType fileType,
        CancellationToken cancellationToken = default);
}
