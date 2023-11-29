using FluentValidation;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration;

namespace TaxBeacon.API.Controllers.Programs.Requests;

public record CreateProgramRequest(
    string Name,
    string? Reference,
    string? Overview,
    string? LegalAuthority,
    string Agency,
    Jurisdiction Jurisdiction,
    string? State,
    string? County,
    string? City,
    string? IncentivesArea,
    string? IncentivesType,
    DateTime? StartDateTimeUtc,
    DateTime? EndDateTimeUtc);

public class CreateProgramRequestValidator: AbstractValidator<CreateProgramRequest>
{
    public CreateProgramRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The program name must contain no more than 100 characters");

        RuleFor(x => x.Reference)
            .MaximumLength(200)
            .WithMessage("The program reference must contain no more than 200 characters");

        RuleFor(x => x.Overview)
            .MaximumLength(1000)
            .WithMessage("The program overview must contain no more than 1000 characters");

        RuleFor(x => x.LegalAuthority)
            .MaximumLength(200)
            .WithMessage("The legal authority must contain no more than 200 characters");

        RuleFor(x => x.Agency)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("The program agency must contain no more than 200 characters");

        RuleFor(x => x.State)
            .MaximumLength(200)
            .WithMessage("The state must contain no more than 200 characters");

        RuleFor(x => x.County)
            .MaximumLength(200)
            .WithMessage("The county must contain no more than 200 characters");

        RuleFor(x => x.City)
            .MaximumLength(200)
            .WithMessage("The city must contain no more than 200 characters");

        RuleFor(x => x.IncentivesArea)
            .MaximumLength(100)
            .WithMessage("The incentives area must contain no more than 100 characters");

        RuleFor(x => x.IncentivesType)
            .MaximumLength(100)
            .WithMessage("The incentives type must contain no more than 100 characters");

        RuleFor(x => x.EndDateTimeUtc)
            .GreaterThanOrEqualTo(x => x.StartDateTimeUtc)
            .When(x => x.StartDateTimeUtc is not null && x.EndDateTimeUtc is not null,
                ApplyConditionTo.CurrentValidator)
            .WithMessage("The program end date must be greater than or equal to the program start date");

        RuleFor(x => x.Jurisdiction)
            .IsInEnum();
    }
}

