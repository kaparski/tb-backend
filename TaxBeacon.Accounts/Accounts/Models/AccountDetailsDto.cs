using Mapster;
using TaxBeacon.Accounts.Models;
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

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public int EntitiesCount { get; init; }

    public int LocationsCount { get; init; }

    public int ContactsCount { get; init; }

    public ClientDetailsDto? Client { get; set; }

    public ReferralDetailsDto? Referral { get; set; }

    public IEnumerable<SalespersonDto> Salespersons { get; init; } = Enumerable.Empty<SalespersonDto>();

    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Account, AccountDetailsDto>()
            .Map(dest => dest.EntitiesCount, src => src.Entities.Count)
            .Map(dest => dest.LocationsCount, src => src.Locations.Count)
            .Map(dest => dest.ContactsCount, src => src.Contacts.Count)
            .Map(dest => dest.Salespersons, 
                src => src.Salespersons.Select(sp => 
                    new { Id = sp.UserId, sp.TenantUser.User.FullName}));
}
