using FluentValidation;

namespace TaxBeacon.API.Controllers.Accounts.Requests;

public record AccountActivityHistoryRequest(int Page, int PageSize);

public class AccountActivityHistoryRequestValidator: AbstractValidator<AccountActivityHistoryRequest>
{
    public AccountActivityHistoryRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("The page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("The page size must be greater than 0.");
    }
}