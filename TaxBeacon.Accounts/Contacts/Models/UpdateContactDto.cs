using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Contacts.Models;

public record UpdateContactDto: IRegister
{
    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? SecondaryEmail { get; init; }

    public string? JobTitle { get; init; }

    public IEnumerable<CreateUpdatePhoneDto> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneDto>();

    public void Register(TypeAdapterConfig config) =>
            config.NewConfig<UpdateContactDto, Contact>()
            .Ignore(x => x.Phones);
}
