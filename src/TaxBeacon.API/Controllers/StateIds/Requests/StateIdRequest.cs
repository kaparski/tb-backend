using FluentValidation;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Controllers.StateIds.Requests;

public record StateIdRequest
{
    public State State { get; init; }

    public StateIdType StateIdType { get; init; } = null!;

    public string StateIdCode { get; init; } = null!;

    public string? LocalJurisdiction { get; init; }
}

public sealed class StateIdRequestValidator: AbstractValidator<StateIdRequest>
{
    public StateIdRequestValidator()
    {
        RuleFor(x => x.StateIdCode).NotEmpty().MaximumLength(25);
        RuleFor(x => x.LocalJurisdiction).MaximumLength(100);
        RuleFor(x => x.StateIdType).NotEqual(StateIdType.None);
        RuleFor(x => x.State).NotEqual(State.None);
    }
}
