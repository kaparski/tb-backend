using FluentValidation;
using System.Security.Cryptography.Xml;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Programs.Requests;

public record UpdateProgramRequest(
    string Name,
    string Reference,
    string Overview,
    string LegalAuthority,
    string Agency,
    Jurisdiction Jurisdiction,
    string State,
    string County,
    string City,
    string IncentivesArea,
    string IncentivesType,
    DateTime StartDateTimeUtc,
    DateTime EndDateTimeUtc);

public class UpdateProgramRequestValidator: AbstractValidator<UpdateProgramRequest>
{
    public UpdateProgramRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("");

        RuleFor(x => x.Reference)
            .MaximumLength(200)
            .WithMessage("");

        RuleFor(x => x.Overview)
            .MaximumLength(200)
            .WithMessage("");

        RuleFor(x => x.LegalAuthority)
            .MaximumLength(200)
            .WithMessage("");

        RuleFor(x => x.Agency)
            .MaximumLength(200)
            .WithMessage("");

        RuleFor(x => x.State)
            .MaximumLength(200)
            .WithMessage("");

        RuleFor(x => x.County)
            .MaximumLength(200)
            .WithMessage("");

        RuleFor(x => x.City)
            .MaximumLength(200)
            .WithMessage("");

        RuleFor(x => x.IncentivesArea)
            .MaximumLength(100)
            .WithMessage("");

        RuleFor(x => x.IncentivesType)
            .MaximumLength(100)
            .WithMessage("");

        RuleFor(x => x.EndDateTimeUtc)
            .GreaterThanOrEqualTo(x => x.StartDateTimeUtc)
            .WithMessage("");
    }
}
