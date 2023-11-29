using FluentValidation;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Controllers.Accounts.Requests;

public sealed record CreateReferralRequest
{
    public required OrganizationType OrganizationType { get; init; }

    public required ReferralType Type { get; init; }

    public IEnumerable<Guid> ReferralManagersIds { get; init; } = Enumerable.Empty<Guid>();
}

public sealed class CreateReferralRequestValidator: AbstractValidator<CreateReferralRequest>
{
    public CreateReferralRequestValidator()
    {
        RuleFor(x => x.OrganizationType)
            .NotNull();

        RuleFor(x => x.Type)
            .NotNull();

        RuleFor(x => x.ReferralManagersIds)
            .NotEmpty();
    }
}
