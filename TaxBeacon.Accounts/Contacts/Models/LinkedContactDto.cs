using Mapster;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Contacts.Models;

public record LinkedContactDto: IRegister
{
    public Guid SourceContactId { get; set; }

    public Guid RelatedContactId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<LinkedContact, LinkedContactDto>()
            .Map(dest => dest.FullName, src => src.RelatedContact.FullName)
            .Map(dest => dest.Email, src => src.RelatedContact.Email);
}
