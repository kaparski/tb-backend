using FluentValidation;

namespace TaxBeacon.API.Controllers.JobTitles.Requests;

public record JobTitleActivityHistoryRequest(int Page, int PageSize);

public class JobTitleActivityHistoryRequestValidator: AbstractValidator<JobTitleActivityHistoryRequest>
{
    public JobTitleActivityHistoryRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}
