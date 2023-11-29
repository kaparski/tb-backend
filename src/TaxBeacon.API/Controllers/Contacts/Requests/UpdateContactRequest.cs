using FluentValidation;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.API.Controllers.Contacts.Requests;

public record UpdateContactRequest: IContactRequest
{
    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? SecondaryEmail { get; init; } = null!;

    public string? JobTitle { get; init; }

    public IEnumerable<CreateUpdatePhoneRequest> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneRequest>();
}

public class UpdateContactRequestValidator: AbstractValidator<UpdateContactRequest>
{
    public UpdateContactRequestValidator(IHttpContextAccessor httpContextAccessor, IAccountDbContext dbContext,
        ICurrentUserService currentUserService) => Include(new ContactRequestValidation(httpContextAccessor, dbContext, currentUserService));
}

