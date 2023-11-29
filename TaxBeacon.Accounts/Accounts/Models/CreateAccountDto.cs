using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Accounts.Models;

public sealed record CreateAccountDto: IRegister
{
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

    public int? PrimaryNaicsCode { get; init; }

    public string AccountId { get; set; } = null!;

    public IEnumerable<Guid> SalespersonIds { get; init; } = Enumerable.Empty<Guid>();

    public IEnumerable<CreateUpdatePhoneDto> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneDto>();

    public CreateClientDto? Client { get; init; }

    public CreateReferralDto? Referral { get; init; }

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<CreateAccountDto, Account>()
            .Ignore(x => x.Phones)
            .Ignore(x => x.Client)
            .Ignore(x => x.Referral);
}




