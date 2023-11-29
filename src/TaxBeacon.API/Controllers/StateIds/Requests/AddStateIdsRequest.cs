using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.API.Extensions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.API.Controllers.StateIds.Requests;

public record AddStateIdsRequest
{
    public List<StateIdRequest> StateIdsToAdd { get; init; } = new();
}

public class AddStateIdsRequestValidator: AbstractValidator<AddStateIdsRequest>
{
    private const int MaxStateIdsCount = 50;

    public AddStateIdsRequestValidator(IHttpContextAccessor httpContextAccessor,
        IAccountDbContext dbContext,
        ICurrentUserService currentUserService) =>
        RuleFor(x => x.StateIdsToAdd)
            .NotEmpty()
            .DependentRules(() =>
                RuleForEach(x => x.StateIdsToAdd)
                    .SetValidator(new StateIdRequestValidator())
                    .DependentRules(() =>
                        RuleFor(x => x.StateIdsToAdd)
                            .Must(x => x.Select(s => s.State).Distinct().Count() == x.Count)
                            .WithMessage("State should be unique")
                            .Must(x => x.Select(s => s.StateIdCode).Distinct().Count() == x.Count)
                            .WithMessage("StateId Code should be unique")))
            .DependentRules(() =>
                RuleFor(x => x.StateIdsToAdd)
                    .CustomAsync(async (list, context, cancellationToken) =>
                    {
                        if (httpContextAccessor.HttpContext.TryGetIdFromRoute("entityId", out var entityId))
                        {
                            var existingEntityStateIds = await dbContext.StateIds
                                .Where(id => id.EntityId == entityId && id.TenantId == currentUserService.TenantId)
                                .ToListAsync(cancellationToken);

                            if (list.Count > MaxStateIdsCount - existingEntityStateIds.Count)
                            {
                                context.AddFailure(nameof(AddStateIdsRequest.StateIdsToAdd),
                                    $"There is a limit of 50 items for State Id, there already exists {existingEntityStateIds.Count}");
                                return;
                            }

                            var newStates = list.Select(stateId => stateId.State).ToList();
                            var existingInEntityIndexes = existingEntityStateIds
                                .Where(existingEntityStateId => newStates.Contains(existingEntityStateId.State))
                                .Select(existingEntityStateId =>
                                    list.FindIndex(stateId => stateId.State == existingEntityStateId.State))
                                .ToList();

                            foreach (var index in existingInEntityIndexes)
                            {
                                context.AddFailure(
                                    $"{nameof(AddStateIdsRequest.StateIdsToAdd)}[{index}].{nameof(StateIdRequest.State)}",
                                    "StateId with the same state already exists");
                            }
                        }

                        var newStatesIdCodes = list.Select(stateIdItem => stateIdItem.StateIdCode).ToList();
                        var existingStateIdsInTenant = await dbContext.StateIds
                            .Where(id => id.TenantId == currentUserService.TenantId
                                         && newStatesIdCodes.Contains(id.StateIdCode))
                            .ToListAsync(cancellationToken);
                        var existingInTenantIndexes = existingStateIdsInTenant
                            .Where(existingEntityStateId => newStatesIdCodes
                                .Contains(existingEntityStateId.StateIdCode, StringComparer.OrdinalIgnoreCase))
                            .Select(existingEntityStateId =>
                                list.FindIndex(stateId => stateId.StateIdCode == existingEntityStateId.StateIdCode))
                            .ToList();

                        foreach (var index in existingInTenantIndexes)
                        {
                            context.AddFailure(
                                $"{nameof(AddStateIdsRequest.StateIdsToAdd)}[{index}].{nameof(StateIdRequest.StateIdCode)}",
                                "Entity with the same State ID code already exists");
                        }
                    }));
}
