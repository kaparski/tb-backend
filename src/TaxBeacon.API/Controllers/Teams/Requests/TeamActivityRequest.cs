using FluentValidation;

namespace TaxBeacon.API.Controllers.Teams.Requests;

public record TeamActivityRequest(int Page, int PageSize);

public class TeamActivityRequestValidator: AbstractValidator<TeamActivityRequest>
{
    public TeamActivityRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
