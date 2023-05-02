using FluentValidation;

namespace TaxBeacon.API.Controllers.Teams.Requests
{
    public record UpdateTeamRequest(string Name, string Description);

    public class UpdateTeamRequestValidator: AbstractValidator<UpdateTeamRequest>
    {
        public UpdateTeamRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("The team name must contain no more than 100 charachters");

            RuleFor(x => x.Description)
                .MaximumLength(200)
                .WithMessage("The team description must contain no more than 200 characters");
        }
    }
}
