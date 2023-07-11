using FluentValidation;
using TaxBeacon.Accounts.Accounts.Models;

namespace TaxBeacon.API.Controllers.Accounts.Requests;

public record UpdateClientRequest(
    decimal? AnnualRevenue,
    int? FoundationYear,
    int? EmployeeCount,
    Guid? PrimaryContactId,
    ICollection<ClientManagerDto>? ClientManagers
    );

public class UpdateClientRequestValidator: AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.FoundationYear)
            .InclusiveBetween(1800, DateTime.UtcNow.Year)
            .WithMessage("Not in years range");

        //RuleFor(x => x.ClientManagers)
        //    .NotEmpty()
        //    .WithMessage("Required field");
    }
};
