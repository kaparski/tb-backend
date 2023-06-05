using FluentValidation;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Controllers.Users.Requests;

public record CreateUserRequest(
    string FirstName,
    string LegalName,
    string LastName,
    string Email,
    Guid? DivisionId,
    Guid? DepartmentId,
    Guid? ServiceAreaId,
    Guid? JobTitleId,
    Guid? TeamId);

public class CreateUserRequestValidator: AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(ICurrentUserService currentUserService)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(64)
            .WithMessage("The email must contain no more than 64 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The first name must contain no more than 100 characters");

        RuleFor(x => x.LegalName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Legal name must contain no more than 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The last name must contain no more than 100 characters");

        RuleFor(x => x.DepartmentId)
            .Empty()
            .When(x => x.DivisionId is null && currentUserService.DivisionEnabled, ApplyConditionTo.CurrentValidator)
            .WithMessage("Cannot set a department without a division.");

        RuleFor(x => x.ServiceAreaId)
            .Empty()
            .When(x => x.DepartmentId is null, ApplyConditionTo.CurrentValidator)
            .WithMessage("Cannot set a service area without a department.");

        RuleFor(x => x.JobTitleId)
            .Empty()
            .When(x => x.DepartmentId is null, ApplyConditionTo.CurrentValidator)
            .WithMessage("Cannot set a job title without a department.");
    }
}
