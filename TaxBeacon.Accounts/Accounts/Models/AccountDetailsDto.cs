using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountDetailsDto(
    Guid Id,
    string Name,
    string? DoingBusinessAs,
    string? LinkedInUrl,
    string Website, 
    string Country, 
    string? StreetAddress1,
    string? StreetAddress2,
    string? City,
    State? State,
    int? Zip,
    string? County,
    int? Phone,
    string? Extension,
    int? Fax,
    string? Address,
    IEnumerable<SalesPersonDto> SalesPersons): IRegister
{
    // TODO: We have to write correct mapping for SalesPersonDto in the future stories
    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Account, AccountDetailsDto>()
            .Map(dest => SalesPersons, src => src.TenantUserAccounts);
}
