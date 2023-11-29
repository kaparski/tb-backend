using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.API.Extensions;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.API.Controllers.Contacts.Requests;

public interface IContactRequest
{
    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string Email { get; init; }

    public string? SecondaryEmail { get; init; }

    public string? JobTitle { get; init; }

    public IEnumerable<CreateUpdatePhoneRequest> Phones { get; init; }
}

public class ContactRequestValidation: AbstractValidator<IContactRequest>
{
    public ContactRequestValidation(IHttpContextAccessor httpContextAccessor, IAccountDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(64)
            .WithMessage("The email must contain no more than 64 characters")
            .DependentRules(() => RuleFor(x => x.Email)
                .MustAsync(async (email, cancellationToken) => !httpContextAccessor.HttpContext.TryGetIdFromRoute("contactId", out var id)
                                                               || !await dbContext.Contacts.AnyAsync(
                                                                   c => c.TenantId == currentUserService.TenantId && c.Email == email && c.Id != id,
                                                                   cancellationToken))
                .WithMessage("Contact with the same email already exists"));

        RuleFor(x => x.SecondaryEmail)
            .EmailAddress()
            .MaximumLength(64)
            .WithMessage("The secondary email must contain no more than 64 characters")
            .When(x => !string.IsNullOrEmpty(x.SecondaryEmail));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The first name must contain no more than 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The last name must contain no more than 100 characters");

        RuleFor(x => x.JobTitle)
            .MaximumLength(100)
            .WithMessage("The job title must contain no more than 100 characters")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        RuleForEach(x => x.Phones)
            .ChildRules(phone =>
                phone.RuleFor(p => p.Type).IsInEnum());

        RuleForEach(x => x.Phones)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.Number).Length(10);
                phone.RuleFor(p => p.Extension)
                    .Null()
                    .When(p => p.Type == PhoneType.Mobile);
            });
    }
}
