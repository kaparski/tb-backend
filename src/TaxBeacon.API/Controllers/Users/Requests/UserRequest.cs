using FluentValidation;

namespace TaxBeacon.API.Controllers.Users.Requests;

public record UserRequest(string FirstName, string LastName, string Email);

public class UserRequestValidator: AbstractValidator<UserRequest>
{
    public UserRequestValidator()
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
