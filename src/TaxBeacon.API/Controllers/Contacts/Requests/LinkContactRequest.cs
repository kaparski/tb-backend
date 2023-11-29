using FluentValidation;

namespace TaxBeacon.API.Controllers.Contacts.Requests;

public sealed record LinkContactRequest
{
    public Guid RelatedContactId { get; set; }

    public string Comment { get; init; } = null!;
}

public sealed class LinkContactRequestValidator: AbstractValidator<LinkContactRequest>
{
    public LinkContactRequestValidator() => RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("The comment must contain no more than {MaxLength}");
}
