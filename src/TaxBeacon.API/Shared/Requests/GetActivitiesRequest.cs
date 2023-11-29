using FluentValidation;

namespace TaxBeacon.API.Shared.Requests;

public record GetActivitiesRequest(int Page, int PageSize);

public class GetActivitiesRequestValidator: AbstractValidator<GetActivitiesRequest>
{
    public GetActivitiesRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
