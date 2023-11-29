using FluentValidation;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.API.Controllers.Contacts.Requests;

public record UpdateAccountContactRequest: UpdateContactRequest
{
    public ContactType Type { get; init; } = null!;
}

public class UpdateAccountContactRequestValidator: AbstractValidator<UpdateAccountContactRequest>
{
    public UpdateAccountContactRequestValidator(IHttpContextAccessor httpContextAccessor, IAccountDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        Include(new ContactRequestValidation(httpContextAccessor, dbContext, currentUserService));
        RuleFor(x => x.Type).NotEqual(ContactType.None);
    }
}
