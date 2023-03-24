using FluentValidation;

namespace TaxBeacon.API.Controllers.Users.Requests;

public record UpdateUserRequest(string FirstName, string LastName);

public class UpdateUserRequestValidator: AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The first name must contain no more than 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The last name must contain no more than 100 characters");
    }
}
