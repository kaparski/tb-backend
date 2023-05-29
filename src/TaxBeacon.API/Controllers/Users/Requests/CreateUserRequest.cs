using FluentValidation;

namespace TaxBeacon.API.Controllers.Users.Requests;

public record CreateUserRequest(
    string FirstName,
    string LegalName,
    string LastName,
    string Email,
    Guid DivisionId,
    Guid DepartmentId,
    Guid ServiceAreaId,
    Guid JobTitleId,
    Guid? TeamId);

public class CreateUserRequestValidator: AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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

        RuleFor(x => x.DivisionId)
            .NotEmpty()
            .WithMessage("Division required");

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("Department required");

        RuleFor(x => x.ServiceAreaId)
            .NotEmpty()
            .WithMessage("ServiceArea required");

        RuleFor(x => x.JobTitleId)
            .NotEmpty()
            .WithMessage("JobTitle required");
    }
}
