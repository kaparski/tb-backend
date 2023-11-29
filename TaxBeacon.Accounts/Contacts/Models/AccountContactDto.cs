using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Contacts.Models;

public record AccountContactDto: IRegister
{
    public Guid Id { get; init; }

    public string FullName { get; set; } = null!;

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? SecondaryEmail { get; init; } = null!;

    public string? JobTitle { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public Status Status { get; init; }

    public string Type { get; init; } = null!;

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();

    public IEnumerable<ContactAccountDto> Accounts { get; init; } = Enumerable.Empty<ContactAccountDto>();

    public IEnumerable<LinkedContactDto> LinkedContacts { get; init; } = Enumerable.Empty<LinkedContactDto>();

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<AccountContact, AccountContactDto>()
            .Map(dest => dest.Id, src => src.ContactId)
            .Map(dest => dest.FullName, src => src.Contact.FullName)
            .Map(dest => dest.FirstName, src => src.Contact.FirstName)
            .Map(dest => dest.LastName, src => src.Contact.LastName)
            .Map(dest => dest.Email, src => src.Contact.Email)
            .Map(dest => dest.SecondaryEmail, src => src.Contact.SecondaryEmail)
            .Map(dest => dest.JobTitle, src => src.Contact.JobTitle)
            .Map(dest => dest.CreatedDateTimeUtc, src => src.Contact.CreatedDateTimeUtc)
            .Map(dest => dest.LastModifiedDateTimeUtc, src => src.Contact.LastModifiedDateTimeUtc)
            .Map(dest => dest.Phones, src => src.Contact.Phones)
            .Map(dest => dest.Accounts, src => src.Contact.Accounts)
            .Map(dest => dest.LinkedContacts, src => src.Contact.LinkedContacts);
}
