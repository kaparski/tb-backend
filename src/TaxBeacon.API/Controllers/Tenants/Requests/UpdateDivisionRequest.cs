using FluentValidation;

namespace TaxBeacon.API.Controllers.Tenants.Requests
{
    public record UpdateDivisionRequest(string Name, string Descriptoion);

    public class UpdateDivisionRequestValidator: AbstractValidator<UpdateDivisionRequest>
    {
        public UpdateDivisionRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("The tenant name must contain no more than 100 characters");

            RuleFor(x => x.Descriptoion)
                .MaximumLength(256)
                .WithMessage("The tenant name must contain no more than 100 characters");
        }
    }
}
