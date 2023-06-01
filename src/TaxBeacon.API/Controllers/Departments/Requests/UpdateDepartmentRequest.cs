using FluentValidation;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Controllers.Departments.Requests;

public record UpdateDepartmentRequest(string Name, string Description, Guid DivisionId, IEnumerable<Guid> ServiceAreasIds, IEnumerable<Guid> JobTitlesIds);

public class UpdateDepartmentRequestValidator: AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator(ICurrentUserService currentUserService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The department name must contain no more than 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("The department description must contain no more than 200 characters");

        if (currentUserService.DivisionEnabled)
        {
            RuleFor(x => x.DivisionId)
                .NotEmpty()
                .WithMessage("Required field");
        }

        RuleFor(x => x.ServiceAreasIds)
            .NotEmpty()
            .WithMessage("Required field");

        RuleFor(x => x.JobTitlesIds)
            .NotEmpty()
            .WithMessage("Required field");
    }
}
