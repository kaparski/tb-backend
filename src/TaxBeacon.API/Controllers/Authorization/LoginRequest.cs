using FluentValidation;

namespace TaxBeacon.API.Controllers.Authorization;

public record LoginRequest(string Email);


public class LoginRequestValidator: AbstractValidator<LoginRequest>
{
    public LoginRequestValidator() =>
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(150)
            .WithMessage("The email must contain no more than 150 characters")
            .NotNull()
            .NotEmpty();
}
