using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.API.Extensions;
using TaxBeacon.DAL.Administration;

namespace TaxBeacon.API.Controllers.Tenants.Requests;

public record UpdateTenantRequest
{
    public string Name { get; init; } = null!;
}

public class UpdateTenantRequestValidator: AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator(IHttpContextAccessor httpContextAccessor,
        ITaxBeaconDbContext dbContext) =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The tenant name must contain no more than 100 characters")
            .DependentRules(() => RuleFor(x => x.Name)
                .MustAsync(async (name, cancellationToken) =>
                    !httpContextAccessor.HttpContext.TryGetIdFromRoute("id", out var id)
                    || !await dbContext.Tenants.AnyAsync(j => j.Name == name
                                                                && j.Id != id,
                        cancellationToken))
                .WithMessage("Tenant with the same name already exists"));

}
