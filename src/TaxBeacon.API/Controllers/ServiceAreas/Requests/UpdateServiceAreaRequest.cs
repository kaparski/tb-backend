using FluentValidation;

namespace TaxBeacon.API.Controllers.ServiceAreas.Requests;

public record UpdateServiceAreaRequest(string Name, string Description);

public class UpdateServiceAreaRequestValidator: AbstractValidator<UpdateServiceAreaRequest>
{
    public UpdateServiceAreaRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The service area name must contain no more than 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("The service area description must contain no more than 200 characters");
    }
}
