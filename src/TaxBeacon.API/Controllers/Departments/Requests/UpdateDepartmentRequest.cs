using FluentValidation;

namespace TaxBeacon.API.Controllers.Departments.Requests;

public record UpdateDepartmentRequest(string Name, string Description);

public class UpdateDepartmentRequestValidator: AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The department name must contain no more than 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("The department description must contain no more than 200 characters");
    }
}
