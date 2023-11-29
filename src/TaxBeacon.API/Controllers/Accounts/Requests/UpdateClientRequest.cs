using FluentValidation;

namespace TaxBeacon.API.Controllers.Accounts.Requests;

public record UpdateClientRequest()
{
    public decimal? AnnualRevenue { get; init; }
    public int? FoundationYear { get; init; }
    public int? EmployeeCount { get; init; }
    public Guid? PrimaryContactId { get; init; }
    public IEnumerable<Guid> ClientManagersIds { get; init; } = Enumerable.Empty<Guid>();
};

public class UpdateClientRequestValidator: AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.FoundationYear)
            .InclusiveBetween(1900, DateTime.UtcNow.Year)
            .WithMessage("Not in years range");

        RuleFor(x => x.FoundationYear)
            .GreaterThan(0)
            .WithMessage("Should be positive number");

        RuleFor(x => x.EmployeeCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Should be positive number");

        RuleFor(x => x.ClientManagersIds)
            .NotEmpty()
            .WithMessage("Required field");
    }
};
