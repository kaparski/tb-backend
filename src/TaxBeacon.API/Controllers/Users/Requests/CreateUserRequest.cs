using FluentValidation;

namespace TaxBeacon.API.Controllers.Users.Requests;

public record CreateUserRequest(string FirstName, string LegalName, string LastName, string Email);

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
    }
}
