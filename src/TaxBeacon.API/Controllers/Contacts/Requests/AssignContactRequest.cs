using FluentValidation;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Controllers.Contacts.Requests;

public record AssignContactRequest
{
    public ContactType Type { get; init; } = null!;
}

public class AssignContactRequestValidator: AbstractValidator<AssignContactRequest>
{
    public AssignContactRequestValidator() => RuleFor(x => x.Type).NotEqual(ContactType.None);
}
