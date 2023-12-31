using FluentValidation;

namespace TaxBeacon.API.Controllers.Tenants.Requests;

public record TenantActivityHistoryRequest(int Page, int PageSize);

public class TenantActivityHistoryRequestValidator: AbstractValidator<TenantActivityHistoryRequest>
{
    public TenantActivityHistoryRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
