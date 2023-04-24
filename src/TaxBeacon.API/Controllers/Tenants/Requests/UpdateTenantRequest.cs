using FluentValidation;

namespace TaxBeacon.API.Controllers.Tenants.Requests;

public record UpdateTenantRequest(string Name);

public class UpdateTenantRequestValidator: AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The tenant name must contain no more than 100 characters");
}
