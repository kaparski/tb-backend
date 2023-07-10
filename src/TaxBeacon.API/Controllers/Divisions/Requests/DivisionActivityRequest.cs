using FluentValidation;

namespace TaxBeacon.API.Controllers.Divisions.Requests;

public record DivisionActivityRequest(uint Page, uint PageSize);

public class DivisionActivityRequestValidator: AbstractValidator<DivisionActivityRequest>
{
    public DivisionActivityRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan((uint)0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan((uint)0)
            .WithMessage("The page size must be greater than 0.");
    }
}
