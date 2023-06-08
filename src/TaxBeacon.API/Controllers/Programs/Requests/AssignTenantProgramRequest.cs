using FluentValidation;

namespace TaxBeacon.API.Controllers.Programs.Requests;

public record AssignTenantProgramRequest(Guid? DepartmentId, Guid? ServiceAreaId);

public class AssignTenantProgramRequestValidator: AbstractValidator<AssignTenantProgramRequest>
{
    public AssignTenantProgramRequestValidator() =>
        RuleFor(x => x.DepartmentId)
            .NotNull()
            .When(x => x.ServiceAreaId is not null, ApplyConditionTo.CurrentValidator);
}