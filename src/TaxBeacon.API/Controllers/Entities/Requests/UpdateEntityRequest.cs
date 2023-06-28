using FluentValidation;
using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.API.Controllers.Entities.Requests;

public record UpdateEntityRequest
(
    string Name,
    string? Dba,
    string? EntityId,
    string? City,
    string StreetAddress1,
    string? StreetAddress2,
    string? Address,
    int Fein,
    int? Zip,
    string Country,
    string? Fax,
    string? Phone,
    string? Extension,
    State State,
    AccountEntityType Type,
    TaxYearEndType TaxYearEndType,
    Status Status,
    IEnumerable<StateId> StateIds
    );

public class UpdateEntityRequestValidator: AbstractValidator<UpdateEntityRequest>
{
    public UpdateEntityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The entity name must contain no more than 100 characters");

        RuleFor(x => x.Dba)
            .MaximumLength(100)
            .WithMessage("The DBA must contain no more than 100 characters");

        RuleFor(x => x.EntityId)
            .MaximumLength(100)
            .WithMessage("The EntityId must contain no more than 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The country name must contain no more than 100 characters");

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("The country name must contain no more than 100 characters");

        RuleFor(x => x.StreetAddress1)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The address must contain no more than 100 characters");

        RuleFor(x => x.StreetAddress2)
            .MaximumLength(100)
            .WithMessage("The address must contain no more than 100 characters");

        RuleFor(x => x.Address)
            .MaximumLength(100)
            .WithMessage("The address must contain no more than 100 characters");

        RuleFor(x => x.Fax)
            .MaximumLength(20)
            .WithMessage("The fax must contain no more than 20 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("The phone number must contain no more than 20 characters");

        RuleFor(x => x.Extension)
            .MaximumLength(20)
            .WithMessage("The extension must contain no more than 20 characters");
    }
}
