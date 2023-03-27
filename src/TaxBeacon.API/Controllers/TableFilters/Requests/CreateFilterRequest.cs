using FluentValidation;

namespace TaxBeacon.API.Controllers.TableFilters.Requests;

public record CreateFilterRequest(string Name, string Configuration);

public class CreateFilterRequestValidator: AbstractValidator<CreateFilterRequest>
{
    public CreateFilterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("The name must contain no more than 50 characters");

        RuleFor(x => x.Configuration)
            .NotEmpty();
    }
}
