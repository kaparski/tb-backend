using FluentValidation;

namespace TaxBeacon.API.Controllers.Accounts.Requests;

public record UpdateClientRequest(
    decimal? AnnualRevenue,
    int? FoundationYear,
    int? EmployeeCount,
    Guid? PrimaryContactId,
    IEnumerable<Guid> ClientManagersIds
    );

public class UpdateClientRequestValidator: AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.FoundationYear)
            .InclusiveBetween(1800, DateTime.UtcNow.Year)
            .WithMessage("Not in years range");

        RuleFor(x => x.ClientManagers)
            .NotEmpty()
            .WithMessage("Required field");
    }
};
