using FluentValidation;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Extensions;

namespace TaxBeacon.API.Controllers.Accounts.Requests;

public record CreateAccountRequest: IAddressRequest, INaicsCodeRequest, IAccountWithAccountIdRequest
{
    public string Name { get; init; } = null!;

    public string? DoingBusinessAs { get; init; }

    public string? LinkedInUrl { get; init; }

    public string Website { get; init; } = null!;

    public Country Country { get; init; } = null!;

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public int? PrimaryNaicsCode { get; init; }

    public string AccountId { get; init; } = null!;

    public CreateClientRequest? Client { get; set; }

    public CreateReferralRequest? Referral { get; set; }

    public IEnumerable<Guid> SalespersonIds { get; init; } = Enumerable.Empty<Guid>();

    public IEnumerable<CreateUpdatePhoneRequest> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneRequest>();
}

public class CreateAccountRequestValidator: AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator(INaicsService naicsService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DoingBusinessAs)
            .MaximumLength(100);

        RuleFor(x => x.LinkedInUrl)
            .WebLink();

        RuleFor(x => x.Website)
            .NotEmpty()
            .WebLink();

        Include(new NaicsCodeRequestValidation(naicsService));

        RuleFor(x => x.SalespersonIds)
            .NotEmpty();

        When(x => x.Client is not null, () =>
        {
            RuleFor(x => x.Client)
                .SetValidator(new CreateClientRequestValidator()!);
            RuleFor(x => x.Referral)
                .Null();
        });

        When(x => x.Referral is not null, () =>
        {
            RuleFor(x => x.Referral)
                .SetValidator(new CreateReferralRequestValidator()!);
            RuleFor(x => x.Client)
                .Null();
        });

        Include(new AccountWithAccountIdRequestValidation());

        Include(new AddressRequestValidation());
    }
}
