using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Contacts.Models;

public record ContactExportModel: IRegister
{
    [Column("First Name")]
    public string FirstName { get; init; } = null!;

    [Column("Last Name")]
    public string LastName { get; init; } = null!;

    [Column("Email")]
    public string Email { get; init; } = null!;

    [Column("Secondary Email")]
    public string SecondaryEmail { get; init; } = null!;

    [Column("Job Title")]
    public string JobTitle { get; init; } = null!;

    [Ignore]
    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();

    [Column("Phone")]
    public string? PhonesView { get; set; } = string.Empty;

    [Ignore]
    public IEnumerable<string> AccountsNames { get; init; } = Enumerable.Empty<string>();

    [Column("Associated Accounts")]
    public string AccountsView { get; set; } = null!;

    [Ignore]
    public IEnumerable<LinkedContactDto> LinkedContacts { get; init; } = Enumerable.Empty<LinkedContactDto>();

    [Column("Linked Contacts")]
    public string? LinkedContactsView { get; set; } = string.Empty;

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Contact, ContactExportModel>()
            .Map(dest => dest.AccountsNames, src => src.Accounts.Select(a => a.Account.Name));
}
