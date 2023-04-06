using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.TableFilters.Requests;

public record CreateTableFilterRequest(string Name, string Configuration, EntityType TableType);

public class CreateTableFilterRequestValidator: AbstractValidator<CreateTableFilterRequest>
{
    public CreateTableFilterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("The name must contain no more than 50 characters");

        // TODO: Add validation for JSON
        RuleFor(x => x.Configuration)
            .NotEmpty();

        RuleFor(x => x.TableType)
            .IsInEnum();
    }
}
