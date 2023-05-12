using FluentValidation;

namespace TaxBeacon.API.Controllers.ServiceAreas.Requests;

public record ServiceAreaActivityHistoryRequest(int Page, int PageSize);

public class ServiceAreaActivityHistoryRequestValidator: AbstractValidator<ServiceAreaActivityHistoryRequest>
{
    public ServiceAreaActivityHistoryRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
