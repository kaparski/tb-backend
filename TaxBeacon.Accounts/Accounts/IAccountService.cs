using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts;

public interface IAccountService
{
    IQueryable<AccountDto> QueryAccounts();

    Task<OneOf<AccountDetailsDto, NotFound>> GetAccountDetailsByIdAsync(Guid id,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportAccountsAsync(FileType fileType,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id,
        AccountInfoType accountInfoType,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    
    Task<OneOf<AccountDetailsDto, NotFound>> UpdateClientStatusAsync(Guid accountId,
        Status status,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default);
}
