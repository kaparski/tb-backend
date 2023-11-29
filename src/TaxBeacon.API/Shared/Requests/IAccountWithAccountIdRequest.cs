using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.API.Shared.Requests;

public interface IAccountWithAccountIdRequest
{
    public string AccountId { get; init; }
}

public class AccountWithAccountIdRequestValidation: AbstractValidator<IAccountWithAccountIdRequest>
{
    public AccountWithAccountIdRequestValidation() =>
        RuleFor(l => l.AccountId)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The account id must contain no more than {MaxLength} characters");
}
