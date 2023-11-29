using FluentValidation;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Controllers.Locations.Requests;

public interface ILocationRequest: IAddressRequest
{
    public string Name { get; init; }

    public string LocationId { get; init; }

    public LocationType Type { get; init; }
}

public class LocationRequestValidation: AbstractValidator<ILocationRequest>
{
    public LocationRequestValidation()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .NotEmpty()
            .NotEqual(LocationType.None);

        Include(new AddressRequestValidation());
    }
}