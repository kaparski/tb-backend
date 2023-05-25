using FluentValidation;

namespace TaxBeacon.API.Controllers.JobTitles.Requests;

public record UpdateJobTitleRequest(string Name, string Description, Guid DepartmentId);

public class UpdateJobTitleRequestValidator: AbstractValidator<UpdateJobTitleRequest>
{
    public UpdateJobTitleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The job title name must contain no more than 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("The job title description must contain no more than 200 characters");

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("The job title must have at least 1 department");
    }
}
