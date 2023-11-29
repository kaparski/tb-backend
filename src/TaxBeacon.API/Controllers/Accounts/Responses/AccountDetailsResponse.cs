using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record AccountDetailsResponse
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

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public NaicsCodeResponse? NaicsCode { get; init; }

    public ClientDetailsResponse? Client { get; set; }

    public ReferralDetailsResponse? Referral { get; set; }

    public IEnumerable<SalespersonResponse> Salespersons { get; init; } = Enumerable.Empty<SalespersonResponse>();

    public IEnumerable<PhoneResponse> Phones { get; init; } = Enumerable.Empty<PhoneResponse>();
}
