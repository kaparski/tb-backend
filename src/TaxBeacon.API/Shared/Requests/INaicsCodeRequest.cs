using FluentValidation;
using TaxBeacon.Accounts.Naics;

namespace TaxBeacon.API.Shared.Requests;

public interface INaicsCodeRequest
{
    public int? PrimaryNaicsCode { get; init; }
}

public class NaicsCodeRequestValidation: AbstractValidator<INaicsCodeRequest>
{
    public NaicsCodeRequestValidation(INaicsService naicsService) =>
        When(x => x.PrimaryNaicsCode is not null, () =>
            RuleFor(x => x.PrimaryNaicsCode)
                .ExclusiveBetween(99999, 1000000)
                .WithMessage("Please enter a six digit number")
                .DependentRules(() =>
                    RuleFor(x => x.PrimaryNaicsCode)
                        .MustAsync(async (_, code, ct) => await naicsService.IsNaicsValidAsync(code, ct))
                        .WithMessage("This NAICS code doesn't exist")
                        .When(x => x.PrimaryNaicsCode.HasValue)));
}
