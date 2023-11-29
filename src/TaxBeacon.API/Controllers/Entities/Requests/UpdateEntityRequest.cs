using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Controllers.Entities.Requests;

public record UpdateEntityRequest: IAddressRequest, INaicsCodeRequest
{
    public string Name { get; init; } = null!;

    public string EntityId { get; init; } = null!;

    public string? DoingBusinessAs { get; init; }

    public Country Country { get; init; } = null!;

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public string? Fein { get; init; }

    public string? Ein { get; init; }

    public string? JurisdictionId { get; init; }

    public AccountEntityType Type { get; init; } = null!;

    public TaxYearEndType? TaxYearEndType { get; init; }

    public DateTime? DateOfIncorporation { get; init; }

    public int? PrimaryNaicsCode { get; init; }

    public IEnumerable<CreateUpdatePhoneRequest> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneRequest>();
}

public class UpdateEntityRequestValidator: AbstractValidator<UpdateEntityRequest>
{
    public UpdateEntityRequestValidator(INaicsService naicsService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The entity name must contain no more than 100 characters");

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The entity id must contain no more than {MaxLength} characters");

        RuleFor(x => x.DoingBusinessAs)
            .MaximumLength(100)
            .WithMessage("The DBA must contain no more than 100 characters");

        RuleFor(x => x.Fein)
            .MaximumLength(9)
            .WithMessage("The fein must contain no more than 9 characters");

        RuleFor(x => x.Ein)
            .MaximumLength(20)
            .WithMessage("The ein must contain no more than 20 characters");

        RuleFor(x => x.JurisdictionId)
            .MaximumLength(20)
            .WithMessage("The jurisdiction Id must contain no more than 20 characters");

        RuleFor(x => x.DateOfIncorporation)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.DateOfIncorporation.HasValue);

        Include(new NaicsCodeRequestValidation(naicsService));

        Include(new AddressRequestValidation());
    }
}
