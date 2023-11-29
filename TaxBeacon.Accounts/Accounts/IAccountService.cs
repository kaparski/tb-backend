using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Accounts.Naics.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts;

public interface IAccountService
{
    IQueryable<AccountDto> QueryAccounts();

    Task<OneOf<AccountDetailsDto, NotFound>> GetAccountDetailsByIdAsync(Guid id,
        AccountInfoType accountInfoType,
        CancellationToken cancellationToken = default);

    Task<OneOf<AccountDetailsDto, InvalidOperation>> CreateAccountAsync(CreateAccountDto createAccountDto,
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

    IQueryable<ClientProspectDto> QueryClientsProspects();

    Task<byte[]> ExportClientsProspectsAsync(FileType fileType, CancellationToken cancellationToken);

    IQueryable<ClientDto> QueryClients();

    Task<byte[]> ExportClientsAsync(FileType fileType, CancellationToken cancellationToken);

    Task<OneOf<AccountDetailsDto, NotFound>> UpdateClientDetailsAsync(Guid accountId,
        UpdateClientDto updatedClient, CancellationToken cancellationToken = default);

    Task<OneOf<AccountDetailsDto, NotFound, InvalidOperation>> UpdateAccountProfileAsync(Guid id,
        UpdateAccountProfileDto updateAccountProfileDto,
        CancellationToken cancellationToken = default);

        Task<OneOf<string, InvalidOperation>> GenerateUniqueAccountIdAsync(CancellationToken cancellationToken);

    IQueryable<ReferralPartnerDto> QueryReferralPartners();

    Task<byte[]> ExportReferralPartnersAsync(FileType fileType, CancellationToken cancellationToken);

    IQueryable<ReferralProspectDto> QueryReferralsProspects();

    Task<byte[]> ExportReferralProspectsAsync(FileType fileType, CancellationToken cancellationToken);

}
