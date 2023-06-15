using FluentValidation;

namespace TaxBeacon.API.Controllers.Entities.Requests;

public record EntityActivityRequest(int Page, int PageSize);

public class EntityActivityRequestValidator: AbstractValidator<EntityActivityRequest>
{
    public EntityActivityRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
