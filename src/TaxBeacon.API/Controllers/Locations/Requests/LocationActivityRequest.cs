using FluentValidation;

namespace TaxBeacon.API.Controllers.Locations.Requests;

public record LocationActivityRequest(int Page, int PageSize);

public class LocationActivityRequestValidator: AbstractValidator<LocationActivityRequest>
{
    public LocationActivityRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
