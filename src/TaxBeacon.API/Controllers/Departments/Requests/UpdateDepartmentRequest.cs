using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.API.Extensions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;

namespace TaxBeacon.API.Controllers.Departments.Requests;

public record UpdateDepartmentRequest
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public Guid? DivisionId { get; init; }

    public IEnumerable<Guid>? ServiceAreasIds { get; init; }

    public IEnumerable<Guid>? JobTitlesIds { get; init; }
}

public class UpdateDepartmentRequestValidator: AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator(IHttpContextAccessor httpContextAccessor,
        ITaxBeaconDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The department name must contain no more than 100 characters")
            .DependentRules(() => RuleFor(x => x.Name)
                .MustAsync(async (name, cancellationToken) =>
                    !httpContextAccessor.HttpContext.TryGetIdFromRoute("id", out var id)
                    || !await dbContext.Departments.AnyAsync(j => j.Name == name
                                                                  && j.TenantId == currentUserService.TenantId
                                                                  && j.Id != id,
                        cancellationToken))
                .WithMessage("Department with the same name already exists"));

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("The department description must contain no more than 200 characters");
    }
}
