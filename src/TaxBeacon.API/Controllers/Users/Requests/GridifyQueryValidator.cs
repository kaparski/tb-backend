using FluentValidation;
using Gridify;

namespace TaxBeacon.API.Controllers.Users.Requests;

public class GridifyQueryValidator: AbstractValidator<GridifyQuery>
{
    public GridifyQueryValidator() => RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(100)
            .GreaterThan(0);
}
