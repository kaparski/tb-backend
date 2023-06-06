using FluentValidation;

namespace TaxBeacon.API.Controllers.Tenants.Requests;

public record ChangeTenantProgramsRequest(IEnumerable<Guid> ProgramsIds);

public class ChangeTenantProgramsRequestValidator: AbstractValidator<ChangeTenantProgramsRequest>
{
    public ChangeTenantProgramsRequestValidator() =>
        RuleFor(r => r.ProgramsIds)
            .NotNull();
}
