using Mapster;
using TaxBeacon.Accounts.Common.Models;
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

    public string AccountId { get; init; } = null!;

    public NaicsCodeDto? NaicsCode { get; init; }

    public ClientDetailsDto? Client { get; set; }

    public ReferralDetailsDto? Referral { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public IEnumerable<SalespersonDto> Salespersons { get; init; } = Enumerable.Empty<SalespersonDto>();

    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Account, AccountDetailsDto>()
            .Map(dest => dest.Salespersons,
                src => src.Salespersons.Select(sp =>
                    new { Id = sp.UserId, sp.TenantUser.User.FullName }));
}
