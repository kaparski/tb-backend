using FluentValidation;

namespace TaxBeacon.API.Controllers.Users.Requests;

public record CreateUserRequest(string FirstName, string LastName, string Email);

public class CreateUserRequestValidator: AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotNull()
            .Matches("^[a-zA-Z-]+$");

        RuleFor(x => x.LastName)
            .NotNull()
            .Matches("^[a-zA-Z-]+$");
    }
}
