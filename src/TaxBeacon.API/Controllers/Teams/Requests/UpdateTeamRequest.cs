using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.API.Extensions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;

namespace TaxBeacon.API.Controllers.Teams.Requests;

public record UpdateTeamRequest
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }
}

public class UpdateTeamRequestValidator: AbstractValidator<UpdateTeamRequest>
{
    public UpdateTeamRequestValidator(IHttpContextAccessor httpContextAccessor,
        ITaxBeaconDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The team name must contain no more than 100 characters")
            .DependentRules(() => RuleFor(x => x.Name)
                .MustAsync(async (name, cancellationToken) =>
                    !httpContextAccessor.HttpContext.TryGetIdFromRoute("id", out var id)
                    || !await dbContext.Teams.AnyAsync(j => j.Name == name
                                                            && j.TenantId == currentUserService.TenantId
                                                            && j.Id != id,
                        cancellationToken))
                .WithMessage("Team with the same name already exists"));

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("The team description must contain no more than 200 characters");
    }
}
