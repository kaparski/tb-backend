using FluentValidation;

namespace TaxBeacon.API.Controllers.Programs.Requests;

public record ProgramActivityHistoryRequest(int Page, int PageSize);

public class ProgramActivityHistoryRequestValidator: AbstractValidator<ProgramActivityHistoryRequest>
{
    public ProgramActivityHistoryRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}