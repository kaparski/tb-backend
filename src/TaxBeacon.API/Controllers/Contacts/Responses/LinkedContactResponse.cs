namespace TaxBeacon.API.Controllers.Contacts.Responses;

public record LinkedContactResponse
{
    public Guid SourceContactId { get; set; }

    public Guid RelatedContactId { get; set; }
}
