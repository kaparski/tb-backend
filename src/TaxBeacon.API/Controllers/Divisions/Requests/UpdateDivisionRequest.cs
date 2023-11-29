using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.API.Extensions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;

namespace TaxBeacon.API.Controllers.Divisions.Requests;

public record UpdateDivisionRequest
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public IEnumerable<Guid>? DepartmentIds { get; init; }
}

public class UpdateDivisionRequestValidator: AbstractValidator<UpdateDivisionRequest>
{
    public UpdateDivisionRequestValidator(IHttpContextAccessor httpContextAccessor,
        ITaxBeaconDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The division name must contain no more than 100 characters")
            .DependentRules(() => RuleFor(x => x.Name)
                .MustAsync(async (name, cancellationToken) =>
                    !httpContextAccessor.HttpContext.TryGetIdFromRoute("id", out var id)
                    || !await dbContext.Divisions.AnyAsync(j => j.Name == name
                                                                && j.TenantId == currentUserService.TenantId
                                                                && j.Id != id,
                        cancellationToken))
                .WithMessage("Division with the same name already exists"));

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("The division description must contain no more than 200 characters");
    }
}
