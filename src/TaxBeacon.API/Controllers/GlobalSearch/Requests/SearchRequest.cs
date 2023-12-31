using FluentValidation;

namespace TaxBeacon.API.Controllers.GlobalSearch.Requests;

public record SearchRequest(string Text, int Page = 1, int PageSize = 5, DateTime? LastUpdatedDateTime = null);

public class SearchRequestValidator: AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotNull()
            .MinimumLength(2);

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0);

        RuleFor(x => x.LastUpdatedDateTime)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.LastUpdatedDateTime is not null);
    }
}
