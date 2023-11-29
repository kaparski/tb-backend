using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.API.Extensions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.API.Controllers.StateIds.Requests;

public record UpdateStateIdRequest: StateIdRequest;

public class UpdateStateIdRequestValidator: AbstractValidator<UpdateStateIdRequest>
{
    public UpdateStateIdRequestValidator(IHttpContextAccessor httpContextAccessor,
        IAccountDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        Include(new StateIdRequestValidator());

        RuleFor(r => r.State)
            .MustAsync(async (state, cancellationToken) =>
                !httpContextAccessor.HttpContext.TryGetIdFromRoute("entityId", out var entityId)
                || !httpContextAccessor.HttpContext.TryGetIdFromRoute("stateId", out var stateId)
                || !await dbContext.StateIds.AnyAsync(s => s.TenantId == currentUserService.TenantId
                                                           && s.EntityId == entityId
                                                           && s.Id != stateId
                                                           && s.State == state, cancellationToken)
            )
            .WithMessage("StateId with the same state already exists");

        RuleFor(r => r.StateIdCode)
            .MustAsync(async (stateIdCode, cancellationToken) =>
                !httpContextAccessor.HttpContext.TryGetIdFromRoute("stateId", out var stateId)
                || !await dbContext.StateIds.AnyAsync(s => s.TenantId == currentUserService.TenantId
                                                           && s.Id != stateId
                                                           && s.StateIdCode == stateIdCode, cancellationToken)
            )
            .WithMessage("Entity with the same State ID code already exists");
    }
}
