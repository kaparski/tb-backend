using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountDetailsDto: IRegister
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? DoingBusinessAs { get; init; }

    public string? LinkedInUrl { get; init; }

    public string Website { get; init; } = null!;

    public string Country { get; init; } = null!;

    public string? StreetAddress1 { get; init; }

    public string? StreetAddress2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Phone { get; init; }

    public string? Extension { get; init; }

    public string? Fax { get; init; }

    public string? Address { get; init; }

    public int EntitiesCount { get; init; }

    public int LocationsCount { get; init; }

    public int ContactsCount { get; init; }

    public ClientDetailsDto? Client { get; set; }

    public ReferralDetailsDto? Referral { get; set; }

    public IEnumerable<SalesPersonDto> SalesPersons { get; init; } = Enumerable.Empty<SalesPersonDto>();

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Account, AccountDetailsDto>()
            .Map(dest => dest.EntitiesCount, src => src.Entities.Count)
            .Map(dest => dest.LocationsCount, src => src.Locations.Count)
            .Map(dest => dest.ContactsCount, src => src.Contacts.Count);
}
