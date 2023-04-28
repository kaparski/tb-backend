using FluentValidation;

namespace TaxBeacon.API.Controllers.Tenants.Requests
{
    public record UpdateDivisionRequest(string Name, string Description);

    public class UpdateDivisionRequestValidator: AbstractValidator<UpdateDivisionRequest>
    {
        public UpdateDivisionRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("The division name must contain no more than 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(256)
                .WithMessage("The division description must contain no more than 256 characters");
        }
    }
}
