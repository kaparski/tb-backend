using FluentValidation;

namespace TaxBeacon.API.Controllers.Departments.Requests;

public record DepartmentActivityHistoryRequest(int Page, int PageSize);

public class DepartmentActivityHistoryRequestValidator: AbstractValidator<DepartmentActivityHistoryRequest>
{
    public DepartmentActivityHistoryRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
