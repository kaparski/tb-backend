using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Contacts.Models;

public record UpdateAccountContactDto: UpdateContactDto, IRegister
{
    public ContactType Type { get; init; } = null!;

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateAccountContactDto, AccountContact>()
            .Map(dest => dest.Contact.FirstName, src => src.FirstName)
            .Map(dest => dest.Contact.LastName, src => src.LastName)
            .Map(dest => dest.Contact.Email, src => src.Email)
            .Map(dest => dest.Contact.SecondaryEmail, src => src.SecondaryEmail)
            .Map(dest => dest.Contact.JobTitle, src => src.JobTitle)
            .Map(dest => dest.Type, src => src.Type.Name);

        config.NewConfig<AccountContact, UpdateAccountContactDto>()
            .Map(dest => dest.FirstName, src => src.Contact.FirstName)
            .Map(dest => dest.LastName, src => src.Contact.LastName)
            .Map(dest => dest.Email, src => src.Contact.Email)
            .Map(dest => dest.SecondaryEmail, src => src.Contact.SecondaryEmail)
            .Map(dest => dest.JobTitle, src => src.Contact.JobTitle)
            .Map(dest => dest.Type, src => ContactType.FromName(src.Type, true));
    }
}
