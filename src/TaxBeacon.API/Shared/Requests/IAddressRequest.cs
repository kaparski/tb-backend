using FluentValidation;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Shared.Requests;

public interface IAddressRequest
{
    Country Country { get; init; }

    string? Address1 { get; init; }

    string? Address2 { get; init; }

    string? City { get; init; }

    State? State { get; init; }

    string? Zip { get; init; }

    string? County { get; init; }

    string? Address { get; init; }

    public IEnumerable<CreateUpdatePhoneRequest> Phones { get; init; }
}

public class AddressRequestValidation: AbstractValidator<IAddressRequest>
{
    public AddressRequestValidation()
    {
        RuleFor(x => x.Address1)
            .MaximumLength(100)
            .WithMessage("The address1 must contain no more than 100 characters");

        RuleFor(x => x.Address2)
            .MaximumLength(100)
            .WithMessage("The address2 must contain no more than 100 characters");

        RuleFor(x => x.County)
            .MaximumLength(150)
            .WithMessage("The county name must contain no more than 150 characters");

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("The city name must contain no more than 100 characters");

        RuleFor(x => x.Address)
            .MaximumLength(200)
            .WithMessage("The address must contain no more than 200 characters");

        RuleFor(x => x.Zip)
            .MaximumLength(10)
            .WithMessage("The zip must contain no more than 10 characters");

        RuleForEach(x => x.Phones)
            .ChildRules(phone =>
                phone.RuleFor(p => p.Type).IsInEnum());

        When(x => x.Country == Country.UnitedStates, () =>
        {
            RuleFor(x => x.Address1).NotEmpty();

            RuleFor(x => x.City).NotEmpty();

            RuleFor(x => x.County).NotEmpty();

            RuleFor(x => x.State).IsInEnum();

            RuleFor(x => x.Zip)
                .NotEmpty()
                .Length(5)
                .Unless(x => x.Zip?.Length == 9, ApplyConditionTo.CurrentValidator);

            RuleFor(x => x.Address).Null();

            RuleForEach(x => x.Phones)
                .ChildRules(phone =>
                {
                    phone.RuleFor(p => p.Number).Length(10);
                    phone.RuleFor(p => p.Extension)
                        .Null()
                        .When(p => p.Type == PhoneType.Mobile);
                });
        }).Otherwise(() =>
        {
            RuleFor(x => x.Address1).Null();

            RuleFor(x => x.Address2).Null();

            RuleFor(x => x.City).Null();

            RuleFor(x => x.County).Null();

            RuleFor(x => x.State).Null();

            RuleFor(x => x.Zip).Null();

            RuleFor(x => x.Address).NotEmpty();
        });
    }
}
