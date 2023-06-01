using FluentValidation;

namespace TaxBeacon.API.Controllers.Users.Requests;

public record UpdateUserRequest(
    string FirstName,
    string LegalName,
    string LastName,
    Guid? DivisionId,
    Guid? DepartmentId,
    Guid? ServiceAreaId,
    Guid? JobTitleId,
    Guid? TeamId);

public class UpdateUserRequestValidator: AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
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
    }
}
